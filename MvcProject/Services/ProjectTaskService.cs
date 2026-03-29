using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using MvcProject.Hubs;
using MvcProject.Models.Domain;
using MvcProject.Models.Enums;
using MvcProject.Repositories.Interfaces;
using MvcProject.Services.Interfaces;
using MvcProject.ViewModels.ProjectTask;
using TaskStatus = MvcProject.Models.Enums.TaskStatus;

namespace MvcProject.Services
{
    public class ProjectTaskService : IProjectTaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public ProjectTaskService(IUnitOfWork unitOfWork, IAuditLogRepository auditLogRepository, IHubContext<NotificationHub> notificationHub)
        {
            _unitOfWork = unitOfWork;
            _auditLogRepository = auditLogRepository;
            _notificationHub = notificationHub;
        }

        public async Task<UserTasksViewModel> GetMyTasksViewModelAsync(string userId, int? projectId, TaskStatus? status)
        {
            var tasks = await _unitOfWork.Tasks.GetAllTasksInUserProjectsAsync(userId);
            var userProjects = await _unitOfWork.Projects.GetProjectsForUserAsync(userId);

            if (projectId.HasValue)
                tasks = tasks.Where(t => t.ProjectId == projectId.Value).ToList();

            if (status.HasValue)
                tasks = tasks.Where(t => t.Status == status.Value).ToList();

            return new UserTasksViewModel
            {
                ToDoTasks = tasks.Where(t => t.Status == TaskStatus.ToDo).ToList(),
                InProgressTasks = tasks.Where(t => t.Status == TaskStatus.InProgress).ToList(),
                ReviewTasks = tasks.Where(t => t.Status == TaskStatus.Review).ToList(),
                DoneTasks = tasks.Where(t => t.Status == TaskStatus.Done).ToList(),

                SelectedProjectId = projectId,
                SelectedStatus = status,
                ProjectList = userProjects.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Title,
                    Selected = projectId == p.Id
                }).ToList(),
                StatusList = Enum.GetValues(typeof(TaskStatus))
                    .Cast<TaskStatus>()
                    .Select(s => new SelectListItem
                    {
                        Value = s.ToString(),
                        Text = s.ToString(),
                        Selected = status == s
                    }).ToList()
            };
        }

        public async Task<ProjectTaskDetailViewModel?> GetTaskDetailViewModelAsync(int taskId, int projectId, string userId)
        {
            var task = await _unitOfWork.Tasks.GetByIdWithAttachmentsAsync(taskId);
            if (task == null || task.ProjectId != projectId) return null;

            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            var assignee = task.AssigneeId != null ? await _unitOfWork.Users.GetByIdAsync(task.AssigneeId) : null;
            var projectUser = await _unitOfWork.ProjectUsers.GetByProjectAndUserAsync(projectId, userId);

            var canEditDelete = projectUser != null &&
                               (projectUser.Role == ProjectRole.Admin || projectUser.Role == ProjectRole.Manager);

            var canChangeStatus = canEditDelete || (task.AssigneeId == userId);

            return new ProjectTaskDetailViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                Deadline = task.Deadline,
                ProjectId = task.ProjectId,
                ProjectTitle = project?.Title ?? "Unknown",
                AssigneeId = task.AssigneeId,
                AssigneeName = assignee != null ? $"{assignee.FirstName} {assignee.LastName}" : "Unassigned",
                AssigneePhotoUrl = assignee?.ProfilePictureUrl,
                CanEditDelete = canEditDelete,
                CanChangeStatus = canChangeStatus,
                Attachments = task.Attachments.Select(a => new AttachmentViewModel
                {
                    Id = a.Id,
                    FileName = a.OriginalFileName,
                    Size = a.Size,
                    ContentType = a.ContentType,
                    UploadedAt = a.UploadedAt
                }).ToList()
            };
        }

        public async Task<ProjectTaskCreateViewModel?> GetCreateTaskViewModelAsync(int projectId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project == null) return null;

            var members = await _unitOfWork.ProjectUsers.GetProjectMembersWithUsersAsync(projectId);

            return new ProjectTaskCreateViewModel
            {
                ProjectId = projectId,
                UsersList = members.Select(m => new SelectListItem
                {
                    Value = m.UserId,
                    Text = $"{m.User.FirstName} {m.User.LastName}"
                })
            };
        }

        public async Task<ProjectTask> CreateTaskAsync(ProjectTaskCreateViewModel model, string actorId)
        {
            var task = new ProjectTask
            {
                Title = model.Title,
                Description = model.Description,
                Status = model.Status,
                Priority = model.Priority,
                Deadline = model.Deadline,
                ProjectId = model.ProjectId,
                AssigneeId = model.AssigneeId
            };

            await _unitOfWork.Tasks.AddAsync(task);
            await _unitOfWork.SaveAsync();

            await LogActivityAsync(task.Id, AuditActionType.Create, "created this task", actorId);

            if (!string.IsNullOrEmpty(task.AssigneeId) && task.AssigneeId != actorId)
            {
                await NotifyAssignmentAsync(task, actorId);
            }

            return task;
        }

        public async Task<ProjectTaskEditViewModel?> GetEditTaskViewModelAsync(int taskId, int projectId)
        {
            var task = await _unitOfWork.Tasks.GetByIdWithAttachmentsAsync(taskId);
            if (task == null || task.ProjectId != projectId) return null;

            var members = await _unitOfWork.ProjectUsers.GetProjectMembersWithUsersAsync(projectId);

            return new ProjectTaskEditViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                Deadline = task.Deadline,
                ProjectId = task.ProjectId,
                AssigneeId = task.AssigneeId,
                UsersList = members.Select(m => new SelectListItem
                {
                    Value = m.UserId,
                    Text = $"{m.User.FirstName} {m.User.LastName}"
                }),
                ExistingAttachments = task.Attachments.Select(a => new AttachmentViewModel
                {
                    Id = a.Id,
                    FileName = a.OriginalFileName,
                    Size = a.Size,
                    ContentType = a.ContentType,
                    UploadedAt = a.UploadedAt
                }).ToList()
            };
        }

        public async Task<bool> UpdateTaskAsync(ProjectTaskEditViewModel model, string actorId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(model.Id);
            if (task == null || task.ProjectId != model.ProjectId) return false;

            var oldAssigneeId = task.AssigneeId;
            var oldStatus = task.Status;

            task.Title = model.Title;
            task.Description = model.Description;
            task.Status = model.Status;
            task.Priority = model.Priority;
            task.Deadline = model.Deadline;
            task.AssigneeId = model.AssigneeId;

            _unitOfWork.Tasks.Update(task);
            await _unitOfWork.SaveAsync();

            if (oldStatus != task.Status)
            {
                await LogActivityAsync(task.Id, AuditActionType.StatusChange, "changed status", actorId, oldStatus.ToString(), task.Status.ToString());
            }
            else if (oldAssigneeId != task.AssigneeId)
            {
                await LogActivityAsync(task.Id, AuditActionType.Assignment, "reassigned this task", actorId);
            }
            else
            {
                await LogActivityAsync(task.Id, AuditActionType.Update, "updated task details", actorId);
            }

            if (!string.IsNullOrEmpty(task.AssigneeId) && task.AssigneeId != oldAssigneeId && task.AssigneeId != actorId)
            {
                await NotifyAssignmentAsync(task, actorId, isUpdate: true);
            }

            return true;
        }

        public async Task<bool> UpdateTaskStatusAsync(int taskId, int projectId, TaskStatus newStatus, string actorId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
            if (task == null || task.ProjectId != projectId) return false;

            var projectUser = await _unitOfWork.ProjectUsers.GetByProjectAndUserAsync(projectId, actorId);
            var canChangeStatus = projectUser != null && 
                (projectUser.Role == ProjectRole.Admin || projectUser.Role == ProjectRole.Manager || task.AssigneeId == actorId);

            if (!canChangeStatus) return false;

            var oldStatus = task.Status;
            task.Status = newStatus;
            
            _unitOfWork.Tasks.Update(task);
            await _unitOfWork.SaveAsync();

            if (oldStatus != newStatus)
            {
                await LogActivityAsync(task.Id, AuditActionType.StatusChange, "changed status", actorId, oldStatus.ToString(), newStatus.ToString());
            }

            return true;
        }

        public async Task<bool> DeleteTaskAsync(int taskId, int projectId)
        {
            var task = await _unitOfWork.Tasks.GetByIdWithAttachmentsAsync(taskId);
            if (task == null || task.ProjectId != projectId) return false;

            _unitOfWork.Tasks.Delete(task);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<AllTasksViewModel> GetAllTasksViewModelAsync(string userId, string searchTerm)
        {
            var tasks = await _unitOfWork.Tasks.GetAllTasksInUserProjectsAsync(userId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                tasks = tasks.Where(t => 
                    (t.Project?.Title != null && t.Project.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) || 
                    t.Status.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (t.Title != null && t.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            var today = DateTime.Today;
            return new AllTasksViewModel
            {
                SearchTerm = searchTerm,
                PresentTasks = tasks.Where(t => t.Deadline == null || t.Deadline.Value.Date >= today)
                                    .OrderBy(t => t.Deadline ?? DateTime.MaxValue)
                                    .ToList(),
                PastTasks = tasks.Where(t => t.Deadline != null && t.Deadline.Value.Date < today)
                                 .OrderByDescending(t => t.Deadline)
                                 .ToList()
            };
        }

        private async Task LogActivityAsync(int taskId, AuditActionType actionType, string description, string userId, string? oldValue = null, string? newValue = null)
        {
            if (string.IsNullOrEmpty(userId)) return;

            var log = new AuditLog
            {
                TaskId = taskId,
                ActionType = actionType,
                UserId = userId,
                Description = description,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedAt = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(log);
            await _unitOfWork.SaveAsync(); // Saves via unit of work even if we pass via AuditLogRepository explicitly to satisfy injection requirement
        }

        private async Task NotifyAssignmentAsync(ProjectTask task, string actorId, bool isUpdate = false)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(task.ProjectId);
            var content = isUpdate 
                ? $"Task \"{task.Title}\" in {project?.Title} has been updated and assigned to you"
                : $"You have been assigned to task \"{task.Title}\" in {project?.Title}";

            var notif = new Notification
            {
                UserId = task.AssigneeId,
                SenderUserId = actorId,
                Type = NotificationType.TaskAssigned,
                Content = content,
                RelatedEntityType = "ProjectTask",
                RelatedEntityId = task.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notif);
            await _unitOfWork.SaveAsync();

            var unreadCount = await _unitOfWork.Notifications.GetUnreadCountAsync(task.AssigneeId);
            await _notificationHub.Clients.Group($"user_{task.AssigneeId}").SendAsync("ReceiveNotification", new
            {
                notif.Id,
                notif.Content,
                Type = "TaskAssigned",
                RelatedEntityType = "ProjectTask",
                RelatedEntityId = task.Id,
                ProjectId = task.ProjectId,
                notif.CreatedAt,
                UnreadCount = unreadCount
            });
        }
    }
}
