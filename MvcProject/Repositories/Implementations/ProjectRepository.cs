using Microsoft.EntityFrameworkCore;

namespace MvcProject.Repositories.Implementations
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        public ProjectRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<int> GetTotalProjectsCountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<List<Project>> GetProjectsForUserAsync(string userId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.Tasks)
                .Where(p => p.Members.Any(m => m.UserId == userId))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Project?> GetProjectWithTasksAsync(int projectId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.Tasks)
                .ThenInclude(t => t.Assignee)
                .Include(p => p.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }
    }
}
