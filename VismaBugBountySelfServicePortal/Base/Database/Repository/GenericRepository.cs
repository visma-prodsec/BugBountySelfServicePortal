using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VismaBugBountySelfServicePortal.Base.Database.DataLayer;
using VismaBugBountySelfServicePortal.Extensions;
using VismaBugBountySelfServicePortal.Models.Entity;

namespace VismaBugBountySelfServicePortal.Base.Database.Repository
{
    public class GenericRepository<T> : IRepository<T> where T : class, IEntity
    {
        private readonly IDatabaseLayer _databaseLayer;
        private readonly DbSet<T> _dbSet;
        private IQueryable<T> _queryable;

        public DbSet<T> DbSet => _dbSet;
        public IQueryable<T> Queryable => _queryable;

        public virtual async Task<IEntity> GetOneEntity(string id)
        {
            return await GetOne(id);
        }

        public virtual void RebuildQueryable(Func<IQueryable<T>, IQueryable<T>> func)
        {
            _queryable = func(_dbSet);
        }

        public void ChainQueryable(Func<IQueryable<T>, IQueryable<T>> func)
        {
            _queryable = func(_queryable);
        }

        public bool SkipSaving { get; set; } = false;

        public GenericRepository(DbSet<T> dbSet, IDatabaseLayer databaseLayer)
        {
            _dbSet = dbSet;
            _queryable = dbSet;
            _databaseLayer = databaseLayer;
        }

        public virtual async Task<T> GetOne(string id)
        {
            if (id == null)
            {
                return null;
            }

            var query = _queryable.Where(e => e.Key == id);
            var entity = await query.FirstOrDefaultAsync();

            return entity;
        }

        public virtual async Task<IEnumerable<T>> GetAll(bool doNotFetch = false)
        {
            var query = _queryable;
            if (!doNotFetch)
            {
                return await query.ToListAsync();
            }

            return query;
        }

        public virtual async Task<PagedResult<T>> GetAllPaginated(int pageNumber, int pageSize, Expression<Func<T, bool>> whereCondition, string sortOrder, bool sortDesc = false
            , Func<T, string> sortFunction = null, IComparer<string> sortComparer = null)
        {
            if(pageSize == -1)
                pageSize = int.MaxValue;
            if (pageSize < 1 || pageNumber < 1)
                throw new Exception("pageNumber and pageSize are required");
            return await _queryable.GetPaged(pageNumber, pageSize, whereCondition, sortOrder, sortDesc, sortFunction, sortComparer);
        }

        public virtual async Task<T> Add(T e)
        {
            var addingResult = await _dbSet.AddAsync(e);

            if (!SkipSaving)
            {
                await _databaseLayer.SaveChangesAsync();
            }

            return addingResult.Entity;
        }

        public virtual async Task<T> AddOrUpdate(T e)
        {
            var existing = await _dbSet.FirstOrDefaultAsync(entity => entity.Key == e.Key);
            if (existing == null)
            {
                return await Add(e);
            }

            return await Update(e);
        }

        public virtual async Task AddOrUpdateMultiple(IEnumerable<T> entities)
        {
            await _databaseLayer.BulkInsertOrUpdate(entities);
        }

        public virtual async Task<T> Update(T e)
        {
            _dbSet.Update(e);
            if (!SkipSaving)
            {
                await _databaseLayer.SaveChangesAsync();
            }

            return e;
        }

        public virtual async Task<bool> Exists(string id)
        {
            var query = _queryable;

            return await query.AnyAsync(e => e.Key == id);
        }

        public virtual async Task<bool> Delete(string id)
        {
            var existing = await GetOne(id);
            if (existing == null)
            {
                return false;
            }

            _dbSet.Remove(existing);

            if (SkipSaving) return true;

            await _databaseLayer.SaveChangesAsync();

            return true;
        }

        public virtual async Task<bool> Delete(T row)
        {
            _dbSet.Remove(row);

            if (SkipSaving) return true;

            await _databaseLayer.SaveChangesAsync();

            return true;
        }

        public virtual async Task<T> FindOne(Expression<Func<T, bool>> predicate)
        {
            return await _queryable.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<IEnumerable<T>> FindAll(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return await _queryable.ToListAsync();
            }

            return await _queryable.Where(predicate).ToListAsync();
        }

        public virtual async Task<bool> Any(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return await _queryable.CountAsync() > 0;
            }

            return await _queryable.AnyAsync(predicate);
        }
    }
}