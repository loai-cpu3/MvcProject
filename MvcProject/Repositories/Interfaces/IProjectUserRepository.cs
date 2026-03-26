using MvcProject.Models.Domain;

namespace MvcProject.Repositories.Interfaces
{
    public interface IProjectUserRepository: IRepository<ProjectUser>
    {
        Task<ProjectUser?> GetByProjectAndUserAsync(int projectId, string userId);
    Task<List<ProjectUser>> GetProjectMembersWithUsersAsync(int projectId);
    Task<ProjectUser?> GetProjectMemberWithUserAsync(int projectId, string userId);
    }
}
