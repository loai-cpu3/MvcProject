using MvcProject.Models.Domain;

namespace MvcProject.Repositories.Interfaces
{
    public interface IProjectRepository: IRepository<Project>
    {
        Task<int> GetTotalProjectsCountAsync();
        Task<List<Project>> GetProjectsForUserAsync(string userId);
        Task<Project?> GetProjectWithTasksAsync(int projectId);
    }
}
