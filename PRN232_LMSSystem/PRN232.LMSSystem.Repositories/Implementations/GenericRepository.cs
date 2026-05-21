using Microsoft.EntityFrameworkCore;
using PRN232.LMSSystem.Repositories.Data;
using PRN232.LMSSystem.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>>? filter = null)
        {
            throw new NotImplementedException();
        }

        public virtual void Delete(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        public Task<IEnumerable<T>> GetAllAsync(System.Linq.Expressions.Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, List<string>? includeProperties = null, int? page = null, int? pageSize = null)
        {
            throw new NotImplementedException();
        }

        public Task<T?> GetByIdAsync(object id, List<string>? includeProperties = null)
        {
            throw new NotImplementedException();
        }

        public Task SaveAsync()
        {
            throw new NotImplementedException();
        }

        public void Update(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
