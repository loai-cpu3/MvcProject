using MvcProject.ViewModels.Projects;

namespace MvcProject.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectIndexViewModel> GetIndexViewModelAsync(string userId);

        Task<ProjectDetailsViewModel?> GetDetailsViewModelAsync(int projectId, string currentUserId);

        Task<ProjectMembersViewModel?> GetMembersViewModelAsync(int projectId);

        Task<AddProjectMembersViewModel?> GetAddMembersViewModelAsync(int projectId, string? searchTerm);

        Task<EditProjectMemberViewModel?> GetEditMemberViewModelAsync(int projectId, string userId);

        Task<int> CreateProjectAsync(CreateProjectViewModel model, string createdById);

        Task<bool> EditProjectAsync(EditProjectViewModel model);

        Task<bool> DeleteProjectAsync(int projectId);

        Task<bool> AddUserToProjectAsync(int projectId, string userId, ProjectRole role);

        Task<bool> UpdateUserRoleInProjectAsync(int projectId, string userId, string actorUserId, ProjectRole newRole);

        Task<bool> RemoveUserFromProjectAsync(int projectId, string userId, string actorUserId);
    }
}
