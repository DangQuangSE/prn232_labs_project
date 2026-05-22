using Microsoft.EntityFrameworkCore;
using PRN232.LMSSystem.Repositories.Data;
using PRN232.LMSSystem.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.LMSSystem.Repositories.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly LmsLab1Context _context;
        private readonly DbSet<T> _dbSet;
        public GenericRepository(LmsLab1Context context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>>? filter = null)
        {
            IQueryable<T> query = _dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.CountAsync();
        }

        public virtual void Delete(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync(
            System.Linq.Expressions.Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            List<string>? includeProperties = null,
            int? page = null,
            int? pageSize = null)
        {
            IQueryable<T> query = _dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    if (!string.IsNullOrWhiteSpace(includeProperty))
                    {
                        query = query.Include(includeProperty);
                    }
                }
            }
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            if (page.HasValue && pageSize.HasValue && page.Value > 0 && pageSize.Value > 0)
            {
                int skip = (page.Value - 1) * pageSize.Value;
                query = query.Skip(skip).Take(pageSize.Value);
            }
            return await query.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(object id, List<string>? includeProperties = null)
        {
            if (includeProperties != null && includeProperties.Any())
            {
                var entityType = _context.Model.FindEntityType(typeof(T));
                var primaryKey = entityType?.FindPrimaryKey();
                var keyName = primaryKey?.Properties.Select(x => x.Name).FirstOrDefault();
                if (keyName != null)
                {
                    IQueryable<T> query = _dbSet;
                    foreach (var includeProperty in includeProperties)
                    {
                        if (!string.IsNullOrWhiteSpace(includeProperty))
                        {
                            query = query.Include(includeProperty.Trim());
                        }
                    }
                    var parameter = Expression.Parameter(typeof(T), "e");
                    var property = Expression.Property(parameter, keyName);
                    var targetKeyProp = primaryKey!.Properties[0];
                    var targetType = targetKeyProp.ClrType;
                    var convertedId = Convert.ChangeType(id, targetType);

                    var equal = Expression.Equal(property, Expression.Constant(convertedId));
                    var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);

                    return await query.FirstOrDefaultAsync(lambda);
                }
            }
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public virtual void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }
}
