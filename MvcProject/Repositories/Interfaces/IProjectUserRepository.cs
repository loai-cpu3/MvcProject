using MvcProject.Models.Domain;

namespace MvcProject.Repositories.Interfaces
{
    public interface IProjectUserRepository: IRepository<ProjectUser>
    {
<<<<<<< HEAD
       
=======
        Task<ProjectUser?> GetByProjectAndUserAsync(int projectId, string userId);
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
    }
}
