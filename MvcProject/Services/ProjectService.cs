using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MvcProject.Models.Domain;
using MvcProject.Services.Interfaces;
using MvcProject.ViewModels.Projects;
using TaskStatus = MvcProject.Models.Enums.TaskStatus;

namespace MvcProject.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
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

        public async Task<ProjectDetailsViewModel?> GetDetailsViewModelAsync(int projectId, string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(currentUserId));
            }

            var project = await _unitOfWork.Projects.GetProjectWithTasksAsync(projectId);
            if (project is null)
            {
                return null;
            }

            var currentUserMembership = await _unitOfWork.ProjectUsers.GetByProjectAndUserAsync(projectId, currentUserId);

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
                IsCurrentUserAdmin = currentUserMembership?.Role == ProjectRole.Admin,
                IsCanCreateTask = currentUserMembership?.Role == ProjectRole.Admin || currentUserMembership?.Role == ProjectRole.Manager,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                CompletionPercentage = completionPercentage,
                ToDoTasks = project.Tasks
                    .Where(task => task.Status == TaskStatus.ToDo)
                    .OrderBy(task => task.Deadline ?? DateTime.MaxValue)
                    .ThenByDescending(task => task.CreatedAt)
                    .Select(task => new ProjectDetailsTaskItemViewModel
                    {
                        Id = task.Id,
                        Title = task.Title,
                        Deadline = task.Deadline,
                        AssigneeAvatarUrl = task.Assignee?.ProfilePictureUrl
                    }).ToList(),
                InProgressTasks = project.Tasks
                    .Where(task => task.Status == TaskStatus.InProgress)
                    .OrderBy(task => task.Deadline ?? DateTime.MaxValue)
                    .ThenByDescending(task => task.CreatedAt)
                    .Select(task => new ProjectDetailsTaskItemViewModel
                    {
                        Id = task.Id,
                        Title = task.Title,
                        Deadline = task.Deadline,
                        AssigneeAvatarUrl = task.Assignee?.ProfilePictureUrl
                    }).ToList(),
                ReviewTasks = project.Tasks
                    .Where(task => task.Status == TaskStatus.Review)
                    .OrderBy(task => task.Deadline ?? DateTime.MaxValue)
                    .ThenByDescending(task => task.CreatedAt)
                    .Select(task => new ProjectDetailsTaskItemViewModel
                    {
                        Id = task.Id,
                        Title = task.Title,
                        Deadline = task.Deadline,
                        AssigneeAvatarUrl = task.Assignee?.ProfilePictureUrl
                    }).ToList(),
                DoneTasks = project.Tasks
                    .Where(task => task.Status == TaskStatus.Done)
                    .OrderByDescending(task => task.CreatedAt)
                    .Select(task => new ProjectDetailsTaskItemViewModel
                    {
                        Id = task.Id,
                        Title = task.Title,
                        Deadline = task.Deadline,
                        AssigneeAvatarUrl = task.Assignee?.ProfilePictureUrl
                    }).ToList(),
                Members = project.Members.Select(m => new ProjectMemberViewModel
                {
                    UserId = m.UserId,
                    FullName = $"{m.User.FirstName} {m.User.LastName}"
                }).ToList(),
                EditProject = new EditProjectViewModel
                {
                    Id = project.Id,
                    Title = project.Title,
                    Description = project.Description
                }
            };
        }

        public async Task<AddProjectMembersViewModel?> GetAddMembersViewModelAsync(int projectId, string? searchTerm)
        {
            if (projectId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
            {
                return null;
            }

            var model = new AddProjectMembersViewModel
            {
                ProjectId = project.Id,
                ProjectTitle = project.Title,
                SearchTerm = searchTerm?.Trim() ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(model.SearchTerm))
            {
                return model;
            }

            var matchedUsers = await _userManager.Users
                .AsNoTracking()
                .Where(user =>
                    user.FirstName.Contains(model.SearchTerm) ||
                    user.LastName.Contains(model.SearchTerm) ||
                    user.Email!.Contains(model.SearchTerm))
                .OrderBy(user => user.FirstName)
                .ThenBy(user => user.LastName)
                .Take(20)
                .ToListAsync();
            var members = await _unitOfWork.ProjectUsers.GetProjectMembersWithUsersAsync(projectId);
            var memberIds = members.Select(member => member.UserId).ToHashSet(StringComparer.Ordinal);

            model.MatchedUsers = matchedUsers.Select(user => new AddProjectMemberUserViewModel
            {
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email,
                AvatarUrl = user.ProfilePictureUrl,
                AlreadyMember = memberIds.Contains(user.Id)
            }).ToList();

            return model;
        }

        public async Task<ProjectMembersViewModel?> GetMembersViewModelAsync(int projectId)
        {
            if (projectId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
            {
                return null;
            }

            var members = await _unitOfWork.ProjectUsers.GetProjectMembersWithUsersAsync(projectId);

            return new ProjectMembersViewModel
            {
                ProjectId = project.Id,
                ProjectTitle = project.Title,
                Members = members.Select(member => new ProjectMemberCardViewModel
                {
                    UserId = member.UserId,
                    FullName = $"{member.User.FirstName} {member.User.LastName}".Trim(),
                    Email = member.User.Email,
                    AvatarUrl = member.User.ProfilePictureUrl,
                    Role = member.Role
                }).ToList()
            };
        }

        public async Task<EditProjectMemberViewModel?> GetEditMemberViewModelAsync(int projectId, string userId)
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
                return null;
            }

            var member = await _unitOfWork.ProjectUsers.GetProjectMemberWithUserAsync(projectId, userId);
            if (member is null)
            {
                return null;
            }

            return new EditProjectMemberViewModel
            {
                ProjectId = project.Id,
                ProjectTitle = project.Title,
                UserId = member.UserId,
                FullName = $"{member.User.FirstName} {member.User.LastName}".Trim(),
                Email = member.User.Email,
                AvatarUrl = member.User.ProfilePictureUrl,
                CurrentRole = member.Role,
                NewRole = member.Role
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

            // Create notification for the added user
            var notif = new Notification
            {
                UserId = userId,
                Type = NotificationType.ProjectMemberAdded,
                Content = $"You have been added to \"{project.Title}\" as {role}",
                RelatedEntityType = "Project",
                RelatedEntityId = projectId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Notifications.AddAsync(notif);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> UpdateUserRoleInProjectAsync(int projectId, string userId, string actorUserId, ProjectRole newRole)
        {
            if (projectId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(actorUserId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(actorUserId));
            }

            if (string.Equals(userId, actorUserId, StringComparison.Ordinal))
            {
                return false;
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

        public async Task<bool> RemoveUserFromProjectAsync(int projectId, string userId, string actorUserId)
        {
            if (projectId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(actorUserId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(actorUserId));
            }

            if (string.Equals(userId, actorUserId, StringComparison.Ordinal))
            {
                return false;
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
