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
    }
}
