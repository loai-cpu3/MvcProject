using Microsoft.EntityFrameworkCore;
using MvcProject.Data;
using MvcProject.Repositories.Interfaces;
using System.Linq.Expressions;

namespace MvcProject.Repositories.Implementations
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

        public async Task<T?> GetByIdAsync(params object[] keyValues)
        {
            T? entity = await _dbSet.FindAsync(keyValues);
            return entity;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            List<T> entities = await _dbSet.ToListAsync();
            return entities;
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
