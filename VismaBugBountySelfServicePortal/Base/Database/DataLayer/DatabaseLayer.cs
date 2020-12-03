using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using VismaBugBountySelfServicePortal.Base.Database.Repository;
using VismaBugBountySelfServicePortal.Models.Entity;

namespace VismaBugBountySelfServicePortal.Base.Database.DataLayer
{
    public class DatabaseLayer : IDatabaseLayer
    {
        private readonly DbContext _dbContext;
        private readonly IDictionary<Type, IDataRepository> _reposCache = new Dictionary<Type, IDataRepository>();


        public DatabaseLayer(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IRepository<T> Repo<T>() where T : class, IEntity
        {
            var type = typeof(T);
            if (_reposCache.ContainsKey(type)) return (IRepository<T>)_reposCache[type];
            var dbSet = _dbContext.Set<T>();
            var repo = new GenericRepository<T>(dbSet, this);
            _reposCache.Add(type, repo);

            return (IRepository<T>)_reposCache[type];
        }

        public IDataRepository Repository(Type entityType)
        {
            var methodInfo = this.GetType().GetMethod(nameof(Repo));
            methodInfo = methodInfo?.MakeGenericMethod(entityType);

            return (IDataRepository)methodInfo?.Invoke(this, new object[] { });
        }

        public async Task BulkInsertOrUpdate<T>(IEnumerable<T> entities) where T : class, IEntity
        {
            await _dbContext.BulkInsertOrUpdateAsync(entities.ToList());
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task MigrateDatabase()
        {
            await _dbContext.Database.MigrateAsync();
        }

        public async Task EnsureMigrated()
        {
            if ((await _dbContext.Database.GetPendingMigrationsAsync()).Count() != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There are pending migrations!");
                Console.WriteLine("run 'dotnet run --migrate true'");
                Console.ForegroundColor = ConsoleColor.White;
                Process.GetCurrentProcess().Kill();
            }
        }

        public void SetRepo<T>(IRepository<T> repo) where T : class, IEntity
        {
            var type = typeof(T);
            if (_reposCache.ContainsKey(type))
            {
                throw new Exception("Repository for '" + type.Name + "' already exists.");
            }

            _reposCache.Add(type, repo);
        }
    }
}