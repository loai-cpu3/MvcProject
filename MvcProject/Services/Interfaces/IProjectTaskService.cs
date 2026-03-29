using MvcProject.Models.Domain;
using MvcProject.ViewModels.ProjectTask;
using TaskStatus = MvcProject.Models.Enums.TaskStatus;

namespace MvcProject.Services.Interfaces
{
    public interface IProjectTaskService
    {
        Task<UserTasksViewModel> GetMyTasksViewModelAsync(string userId, int? projectId, TaskStatus? status);
        Task<ProjectTaskDetailViewModel?> GetTaskDetailViewModelAsync(int taskId, int projectId, string userId);
        Task<ProjectTaskCreateViewModel?> GetCreateTaskViewModelAsync(int projectId);
        Task<ProjectTask> CreateTaskAsync(ProjectTaskCreateViewModel model, string actorId);
        Task<ProjectTaskEditViewModel?> GetEditTaskViewModelAsync(int taskId, int projectId);
        Task<bool> UpdateTaskAsync(ProjectTaskEditViewModel model, string actorId);
        Task<bool> UpdateTaskStatusAsync(int taskId, int projectId, TaskStatus newStatus, string actorId);
        Task<bool> DeleteTaskAsync(int taskId, int projectId);
        Task<AllTasksViewModel> GetAllTasksViewModelAsync(string userId, string searchTerm);
    }
}
