using MvcProject.Models.Domain;

namespace MvcProject.Repositories.Interfaces
{
    public interface ITaskRepository: IRepository<ProjectTask>
    {
<<<<<<< HEAD
=======
        Task<double> GetUserWeeklyCompletionRateAsync(int projectId,string userId);
        Task<int> GetTotalPendingTasksCountAsync();
        Task<List<ProjectTask>> GetRecentTasksAsync(int count);
        
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
    }
}
