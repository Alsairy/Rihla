using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Rihla.Core.Entities;
using Rihla.Core.Interfaces;
using Rihla.Infrastructure.Data;

namespace Rihla.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        public virtual async Task<T?> GetByIdAsync(int id, string tenantId)
        {
            if (typeof(T).IsSubclassOf(typeof(TenantEntity)))
            {
                return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted && 
                    ((TenantEntity)(object)e).TenantId == int.Parse(tenantId));
            }
            return await GetByIdAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.Where(e => !e.IsDeleted).ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(string tenantId)
        {
            if (typeof(T).IsSubclassOf(typeof(TenantEntity)))
            {
                return await _dbSet.Where(e => !e.IsDeleted && 
                    ((TenantEntity)(object)e).TenantId == int.Parse(tenantId)).ToListAsync();
            }
            return await GetAllAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => !e.IsDeleted).Where(predicate).ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string tenantId)
        {
            if (typeof(T).IsSubclassOf(typeof(TenantEntity)))
            {
                return await _dbSet.Where(e => !e.IsDeleted && 
                    ((TenantEntity)(object)e).TenantId == int.Parse(tenantId)).Where(predicate).ToListAsync();
            }
            return await FindAsync(predicate);
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => !e.IsDeleted).FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, string tenantId)
        {
            if (typeof(T).IsSubclassOf(typeof(TenantEntity)))
            {
                return await _dbSet.Where(e => !e.IsDeleted && 
                    ((TenantEntity)(object)e).TenantId == int.Parse(tenantId)).FirstOrDefaultAsync(predicate);
            }
            return await FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => !e.IsDeleted).AnyAsync(predicate);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, string tenantId)
        {
            if (typeof(T).IsSubclassOf(typeof(TenantEntity)))
            {
                return await _dbSet.Where(e => !e.IsDeleted && 
                    ((TenantEntity)(object)e).TenantId == int.Parse(tenantId)).AnyAsync(predicate);
            }
            return await AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync()
        {
            return await _dbSet.Where(e => !e.IsDeleted).CountAsync();
        }

        public virtual async Task<int> CountAsync(string tenantId)
        {
            if (typeof(T).IsSubclassOf(typeof(TenantEntity)))
            {
                return await _dbSet.Where(e => !e.IsDeleted && 
                    ((TenantEntity)(object)e).TenantId == int.Parse(tenantId)).CountAsync();
            }
            return await CountAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => !e.IsDeleted).CountAsync(predicate);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate, string tenantId)
        {
            if (typeof(T).IsSubclassOf(typeof(TenantEntity)))
            {
                return await _dbSet.Where(e => !e.IsDeleted && 
                    ((TenantEntity)(object)e).TenantId == int.Parse(tenantId)).CountAsync(predicate);
            }
            return await CountAsync(predicate);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            await _dbSet.AddRangeAsync(entities);
            return entities;
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            entity.MarkAsUpdated(entity.UpdatedBy ?? "System");
            _dbSet.Update(entity);
            return await Task.FromResult(entity);
        }

        public virtual async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.MarkAsUpdated(entity.UpdatedBy ?? "System");
            }
            _dbSet.UpdateRange(entities);
            return await Task.FromResult(entities);
        }

        public virtual async Task DeleteAsync(int id, string deletedBy)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity, deletedBy);
            }
        }

        public virtual async Task DeleteAsync(int id, string tenantId, string deletedBy)
        {
            var entity = await GetByIdAsync(id, tenantId);
            if (entity != null)
            {
                await DeleteAsync(entity, deletedBy);
            }
        }

        public virtual async Task DeleteAsync(T entity, string deletedBy)
        {
            entity.MarkAsDeleted(deletedBy);
            await UpdateAsync(entity);
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, string deletedBy)
        {
            foreach (var entity in entities)
            {
                entity.MarkAsDeleted(deletedBy);
            }
            await UpdateRangeAsync(entities);
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted);
        }

        public virtual async Task<bool> ExistsAsync(int id, string tenantId)
        {
            if (typeof(T).IsSubclassOf(typeof(TenantEntity)))
            {
                return await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted && 
                    ((TenantEntity)(object)e).TenantId == int.Parse(tenantId));
            }
            return await ExistsAsync(id);
        }

        public virtual IQueryable<T> Query()
        {
            return _dbSet.Where(e => !e.IsDeleted);
        }

        public virtual IQueryable<T> Query(string tenantId)
        {
            if (typeof(T).IsSubclassOf(typeof(TenantEntity)))
            {
                return _dbSet.Where(e => !e.IsDeleted && 
                    ((TenantEntity)(object)e).TenantId == int.Parse(tenantId));
            }
            return Query();
        }

        public virtual IQueryable<T> QueryWithIncludes(params Expression<Func<T, object>>[] includes)
        {
            var query = Query();
            return includes.Aggregate(query, (current, include) => current.Include(include));
        }

        public virtual IQueryable<T> QueryWithIncludes(string tenantId, params Expression<Func<T, object>>[] includes)
        {
            var query = Query(tenantId);
            return includes.Aggregate(query, (current, include) => current.Include(include));
        }
    }
}
