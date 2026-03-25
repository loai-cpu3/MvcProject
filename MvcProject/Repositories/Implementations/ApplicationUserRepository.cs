using Microsoft.EntityFrameworkCore;
using MvcProject.Data;
using MvcProject.Models.Domain;
using MvcProject.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MvcProject.Repositories.Implementations
{
    public class ApplicationUserRepository : IApplicationUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<ApplicationUser> _dbSet;

        public ApplicationUserRepository(ApplicationDbContext context)
        {

            _dbSet = _context.Set<ApplicationUser>();
        }

        public async Task<ApplicationUser?> GetByIdAsync(params object[] keyValues)
        {
            return await _dbSet.FindAsync(keyValues);
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task AddAsync(ApplicationUser entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(ApplicationUser entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(ApplicationUser entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
