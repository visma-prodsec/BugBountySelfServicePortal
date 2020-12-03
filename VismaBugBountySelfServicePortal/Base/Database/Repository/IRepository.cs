using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VismaBugBountySelfServicePortal.Models.Entity;

namespace VismaBugBountySelfServicePortal.Base.Database.Repository
{
    public interface IRepository<T> : IDataRepository where T : class, IEntity
    {
        Task<T> GetOne(string id);
        Task<IEnumerable<T>> GetAll(bool doNotFetch = false);
        Task<PagedResult<T>> GetAllPaginated(int pageNumber, int pageSize, Expression<Func<T, bool>> whereCondition, string sortOrder, bool sortDesc = false
            , Func<T, string> sortFunction = null, IComparer<string> sortComparer = null);
        Task<T> Add(T e);
        Task<T> Update(T e);
        Task<T> AddOrUpdate(T e);
        Task<T> FindOne(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAll(Expression<Func<T, bool>> predicate = null);
        Task<bool> Any(Expression<Func<T, bool>> predicate = null);
        Task<bool> Exists(string id);

        Task<bool> Delete(string id);
        Task<bool> Delete(T row);
        DbSet<T> DbSet { get; }
        IQueryable<T> Queryable { get; }
        void RebuildQueryable(Func<IQueryable<T>, IQueryable<T>> func);
        void ChainQueryable(Func<IQueryable<T>, IQueryable<T>> func);
        Task AddOrUpdateMultiple(IEnumerable<T> entities);
    }
}