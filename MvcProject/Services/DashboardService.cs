using MvcProject.Services.Interfaces;
using MvcProject.ViewModels.Home;

namespace MvcProject.Services
{
    public class DashboardService : IDashboardService
    {
        private IUnitOfWork _unitOfWork;
        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
            }

            var projects = await _unitOfWork.Projects.GetProjectsForUserAsync(userId);
            var completionRates = new List<double>(projects.Count);
            foreach (var project in projects)
            {
                var rate = await _unitOfWork.Tasks.GetUserWeeklyCompletionRateAsync(project.Id, userId);
                completionRates.Add(rate);
            }
            var totalPendingTasks = await _unitOfWork.Tasks.GetTotalPendingTasksCountAsync(userId);
            var recentTasks = await _unitOfWork.Tasks.GetRecentTasksAsync(userId, 3);

            return new DashboardViewModel()
            {
                CompletionRate = completionRates.Count == 0 ? 0 : Math.Round(completionRates.Average(), 2),
                TotalPendingTasks = totalPendingTasks,
                TotalProjects = projects.Count,
                RecentTasks = recentTasks
                    .Select(task => new DashboardTaskViewModel
                    {
                        Id = task.Id,
                        Title = task.Title,
                        ProjectName = task.Project.Title,
                        Status = task.Status,
                        DueDate = DateOnly.FromDateTime(task.Deadline ?? task.CreatedAt)
                    })
                    .ToList()
            };
        }

    }
}
