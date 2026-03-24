using MvcProject.Models.Domain;

namespace MvcProject.Repositories.Interfaces
{
    public interface IProjectUserRepository: IRepository<ProjectUser>
    {
        Task<ProjectUser?> GetByProjectAndUserAsync(int projectId, string userId);
    }
}
