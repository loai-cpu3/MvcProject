using MvcProject.Models.Domain;

namespace MvcProject.Repositories.Interfaces
{
    public interface ITaskRepository: IRepository<ProjectTask>
    {
        Task<double> GetUserWeeklyCompletionRateAsync(int projectId,string userId);
        Task<int> GetTotalPendingTasksCountAsync(string userId);
        Task<List<ProjectTask>> GetRecentTasksAsync(string userId, int count);
        
    }
}
