using System.Linq.Expressions;
using Rihla.Core.Entities;

namespace Rihla.Core.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdAsync(int id, string tenantId);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(string tenantId);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string tenantId);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, string tenantId);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, string tenantId);
        Task<int> CountAsync();
        Task<int> CountAsync(string tenantId);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate, string tenantId);
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
        Task<T> UpdateAsync(T entity);
        Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities);
        Task DeleteAsync(int id, string deletedBy);
        Task DeleteAsync(int id, string tenantId, string deletedBy);
        Task DeleteAsync(T entity, string deletedBy);
        Task DeleteRangeAsync(IEnumerable<T> entities, string deletedBy);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsAsync(int id, string tenantId);
        IQueryable<T> Query();
        IQueryable<T> Query(string tenantId);
        IQueryable<T> QueryWithIncludes(params Expression<Func<T, object>>[] includes);
        IQueryable<T> QueryWithIncludes(string tenantId, params Expression<Func<T, object>>[] includes);
    }
}
