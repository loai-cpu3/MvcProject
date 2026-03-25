using MvcProject.Models.Domain;

namespace MvcProject.Repositories.Interfaces
{
    public interface IProjectRepository: IRepository<Project>
    {
<<<<<<< HEAD
=======
        Task<int> GetTotalProjectsCountAsync();
        Task<List<Project>> GetProjectsForUserAsync(string userId);
        Task<Project?> GetProjectWithTasksAsync(int projectId);
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
    }
}
