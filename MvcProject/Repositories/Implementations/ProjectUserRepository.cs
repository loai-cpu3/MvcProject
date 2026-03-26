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

        public async Task<List<ProjectUser>> GetProjectMembersWithUsersAsync(int projectId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(pu => pu.User)
                .Where(pu => pu.ProjectId == projectId)
                .OrderBy(pu => pu.User.FirstName)
                .ThenBy(pu => pu.User.LastName)
                .ToListAsync();
        }

        public async Task<ProjectUser?> GetProjectMemberWithUserAsync(int projectId, string userId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(pu => pu.User)
                .FirstOrDefaultAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);
        }
    }
}
