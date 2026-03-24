using MvcProject.Models.Domain;

namespace MvcProject.Repositories.Interfaces
{
    public interface ITaskRepository: IRepository<ProjectTask>
    {
        Task<double> GetUserWeeklyCompletionRateAsync(int projectId,string userId);
        Task<int> GetTotalPendingTasksCountAsync();
        Task<List<ProjectTask>> GetRecentTasksAsync(int count);
        
    }
}
