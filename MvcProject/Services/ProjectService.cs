using MvcProject.Services.Interfaces;
using MvcProject.ViewModels.Projects;
using TaskStatus = MvcProject.Models.Enums.TaskStatus;

namespace MvcProject.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProjectService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProjectIndexViewModel> GetIndexViewModelAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
            }

            var projects = await _unitOfWork.Projects.GetProjectsForUserAsync(userId);

            var model = new ProjectIndexViewModel
            {
                Projects = projects.Select(project =>
                {
                    var totalTasks = project.Tasks.Count;
                    var completedTasks = project.Tasks.Count(task => task.Status == TaskStatus.Done);
                    var completionPercentage = totalTasks == 0
                        ? 0
                        : (int)Math.Round(completedTasks * 100d / totalTasks);

                    return new ProjectIndexItemViewModel
                    {
                        ProjectId = project.Id,
                        Title = project.Title,
                        Description = project.Description,
                        CompletionPercentage = completionPercentage
                    };
                }).ToList()
            };

            return model;
        }

        public async Task<ProjectDetailsViewModel?> GetDetailsViewModelAsync(int projectId)
        {
            var project = await _unitOfWork.Projects.GetProjectWithTasksAsync(projectId);
            if (project is null)
            {
                return null;
            }

            var totalTasks = project.Tasks.Count;
            var completedTasks = project.Tasks.Count(task => task.Status == TaskStatus.Done);
            var completionPercentage = totalTasks == 0
                ? 0
                : (int)Math.Round(completedTasks * 100d / totalTasks);

            return new ProjectDetailsViewModel
            {
                ProjectId = project.Id,
                Title = project.Title,
                Description = project.Description,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                CompletionPercentage = completionPercentage,
                Tasks = project.Tasks.Select(task => new ProjectTaskItemViewModel
                {
                    Id = task.Id,
                    Title = task.Title,
                    Deadline = task.Deadline,
                    Status = task.Status,
                    Priority = task.Priority,
                    AssigneeName = task.Assignee != null ? $"{task.Assignee.FirstName} {task.Assignee.LastName}" : "Unassigned",
                    AssigneePhotoUrl = task.Assignee?.ProfilePictureUrl
                }).ToList(),
                EditProject = new EditProjectViewModel
                {
                    Id = project.Id,
                    Title = project.Title,
                    Description = project.Description
                }
            };
        }

        public async Task<int> CreateProjectAsync(CreateProjectViewModel model, string createdById)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (string.IsNullOrWhiteSpace(createdById))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(createdById));
            }

            var project = new Project
            {
                Title = model.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                CreatedById = createdById
            };

            await _unitOfWork.Projects.AddAsync(project);
            await _unitOfWork.SaveAsync();

            var membership = new ProjectUser
            {
                ProjectId = project.Id,
                UserId = createdById,
                Role = ProjectRole.Admin
            };

            await _unitOfWork.ProjectUsers.AddAsync(membership);
            await _unitOfWork.SaveAsync();

            return project.Id;
        }

        public async Task<bool> EditProjectAsync(EditProjectViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var project = await _unitOfWork.Projects.GetByIdAsync(model.Id);
            if (project is null)
            {
                return false;
            }

            project.Title = model.Title.Trim();
            project.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();

            _unitOfWork.Projects.Update(project);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteProjectAsync(int projectId)
        {
            if (projectId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
            {
                return false;
            }

            _unitOfWork.Projects.Delete(project);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> AddUserToProjectAsync(int projectId, string userId, ProjectRole role)
        {
            if (projectId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
            }

            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
            {
                return false;
            }

            var existingMembership = await _unitOfWork.ProjectUsers.GetByProjectAndUserAsync(projectId, userId);
            if (existingMembership is not null)
            {
                existingMembership.Role = role;
                _unitOfWork.ProjectUsers.Update(existingMembership);
                await _unitOfWork.SaveAsync();
                return true;
            }

            await _unitOfWork.ProjectUsers.AddAsync(new ProjectUser
            {
                ProjectId = projectId,
                UserId = userId,
                Role = role
            });
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> UpdateUserRoleInProjectAsync(int projectId, string userId, ProjectRole newRole)
        {
            if (projectId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
            }

            var membership = await _unitOfWork.ProjectUsers.GetByProjectAndUserAsync(projectId, userId);
            if (membership is null)
            {
                return false;
            }

            membership.Role = newRole;
            _unitOfWork.ProjectUsers.Update(membership);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> RemoveUserFromProjectAsync(int projectId, string userId)
        {
            if (projectId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
            }

            var membership = await _unitOfWork.ProjectUsers.GetByProjectAndUserAsync(projectId, userId);
            if (membership is null)
            {
                return false;
            }

            _unitOfWork.ProjectUsers.Delete(membership);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}
