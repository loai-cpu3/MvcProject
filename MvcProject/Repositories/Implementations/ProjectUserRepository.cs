using Microsoft.EntityFrameworkCore;

namespace MvcProject.Repositories.Implementations
{
    public class ProjectUserRepository: Repository<ProjectUser>, IProjectUserRepository
    {
        public ProjectUserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ProjectUser?> GetByProjectAndUserAsync(int projectId, string userId)
        {
            return await _dbSet.FirstOrDefaultAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);
        }
    }
}
