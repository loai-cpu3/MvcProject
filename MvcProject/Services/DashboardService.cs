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

            var recentActivities = await _unitOfWork.AuditLogs.GetRecentProjectActivitiesAsync(userId, 10);

            return new DashboardViewModel()
            {
                CompletionRate = completionRates.Count == 0 ? 0 : Math.Round(completionRates.Average(), 2),
                TotalPendingTasks = totalPendingTasks,
                TotalProjects = projects.Count,
                RecentTasks = recentTasks
                    .Select(task => new DashboardTaskViewModel
                    {
                        Id = task.Id,
                        ProjectId = task.ProjectId,
                        Title = task.Title,
                        ProjectName = task.Project.Title,
                        Status = task.Status,
                        DueDate = DateOnly.FromDateTime(task.Deadline ?? task.CreatedAt)
                    })
                    .ToList(),
                RecentActivities = recentActivities
                    .Select(a => new DashboardActivityViewModel
                    {
                        Id = a.Id,
                        ActionType = a.ActionType.ToString(),
                        Description = a.Description,
                        UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "System",
                        UserInitials = a.User != null ? a.User.FirstName.Substring(0, 1).ToUpper() : "S",
                        UserAvatarUrl = a.User?.ProfilePictureUrl,
                        TaskTitle = a.Task?.Title ?? "Unknown Task",
                        TaskId = a.TaskId,
                        ProjectId = a.Task?.ProjectId ?? 0,
                        ProjectTitle = a.Task?.Project?.Title ?? "Unknown Project",
                        OldValue = a.OldValue,
                        NewValue = a.NewValue,
                        CreatedAt = a.CreatedAt
                    })
                    .ToList()
            };
        }

    }
}
