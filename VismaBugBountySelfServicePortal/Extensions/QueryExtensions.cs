using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VismaBugBountySelfServicePortal.Models.Entity;

namespace VismaBugBountySelfServicePortal.Extensions
{
    public static class QueryExtensions
    {
        public static async Task<PagedResult<T>> GetPaged<T>(this IQueryable<T> query, int pageNumber, int pageSize, Expression<Func<T, bool>> whereCondition, string sortOrder
            , bool sortDesc = false, Func<T,string> sortFunction = null, IComparer<string> sortComparer = null) where T : class
        {
            var result = new PagedResult<T>
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                RowCount = query.Where(whereCondition).Count()
            };


            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);

            var skip = (pageNumber - 1) * pageSize;
            
            result.Results = await query.Where(whereCondition).Sort(sortOrder, sortDesc, sortFunction, sortComparer).Skip(skip).Take(pageSize).ToListAsyncSafe();
            return result;
        }

        /// <summary>
        /// Extension method to sort dynamically based on a attribute referenced by a string.
        /// </summary>
        /// <typeparam name="T">The type of the IEnumerable collection we are working with.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="sortDesc">The direction to perform the sort.</param>
        /// <param name="sortFunction"></param>
        /// <param name="sortComparer"></param>
        private static IQueryable<T> Sort<T>(this IEnumerable<T> source, string attribute, bool sortDesc, Func<T, string> sortFunction = null, IComparer<string> sortComparer = null)
        {
            if (sortFunction != null && sortComparer!=null)
            {
                return sortDesc ? source.AsQueryable().OrderByDescending(sortFunction, sortComparer).AsQueryable() : source.AsQueryable().OrderBy(sortFunction, sortComparer).AsQueryable();
            }
            if (string.IsNullOrWhiteSpace(attribute))
                return source.AsQueryable();
            var param = Expression.Parameter(typeof(T), "item");

            var sortExpression = Expression.Lambda<Func<T, object>>
            (
                Expression.Convert
                (
                    Expression.Property(param, attribute),
                    typeof(object)
                ),
                param
            );

            return sortDesc ? source.AsQueryable().OrderByDescending(sortExpression)
                : source.AsQueryable().OrderBy(sortExpression);

        }
    }

    public static class EfExtensions
    {
        public static Task<List<TSource>> ToListAsyncSafe<TSource>(
            this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!(source is IAsyncEnumerable<TSource>))
                return Task.FromResult(source.ToList());
            return source.ToListAsync();
        }
    }
}
