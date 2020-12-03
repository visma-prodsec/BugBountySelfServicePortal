using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VismaBugBountySelfServicePortal.Base.Database.Repository;
using VismaBugBountySelfServicePortal.Models.Entity;

namespace VismaBugBountySelfServicePortal.Base.Database.DataLayer
{
    public interface IDatabaseLayer
    {
        IRepository<T> Repo<T>() where T : class, IEntity;
        IDataRepository Repository(Type entityType);
        Task BulkInsertOrUpdate<T>(IEnumerable<T> entities) where T : class, IEntity;
        Task SaveChangesAsync();
        Task MigrateDatabase();
        Task EnsureMigrated();
        void SetRepo<T>(IRepository<T> repo) where T : class, IEntity;
    }
}