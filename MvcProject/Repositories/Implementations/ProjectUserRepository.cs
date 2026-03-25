<<<<<<< HEAD
=======
using Microsoft.EntityFrameworkCore;

>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
namespace MvcProject.Repositories.Implementations
{
    public class ProjectUserRepository: Repository<ProjectUser>, IProjectUserRepository
    {
        public ProjectUserRepository(ApplicationDbContext context) : base(context)
        {
        }
<<<<<<< HEAD
       
=======

        public async Task<ProjectUser?> GetByProjectAndUserAsync(int projectId, string userId)
        {
            return await _dbSet.FirstOrDefaultAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);
        }
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
    }
}
