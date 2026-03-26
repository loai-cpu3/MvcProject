using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Attributes;
using MvcProject.Services.Interfaces;
using MvcProject.ViewModels.Projects;
using System.Security.Claims;

namespace MvcProject.Controllers
{


    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var model = await _projectService.GetIndexViewModelAsync(userId);
            return View(model);
        }

        [ProjectAuthorize(ProjectRole.Admin)]
        public async Task<IActionResult> AddMembers([FromQuery] int projectId, string? searchTerm)
        {
            var model = await _projectService.GetAddMembersViewModelAsync(projectId, searchTerm);
            if (model is null)
            {
                return NotFound();
            }

            return View(model);
        }

        [ProjectAuthorize(ProjectRole.Admin)]
        public async Task<IActionResult> Members(int projectId)
        {
            var model = await _projectService.GetMembersViewModelAsync(projectId);
            if (model is null)
            {
                return NotFound();
            }

            return View(model);
        }

        [ProjectAuthorize(ProjectRole.Admin)]
        public async Task<IActionResult> EditMember(int projectId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest();
            }

            var model = await _projectService.GetEditMemberViewModelAsync(projectId, userId);
            if (model is null)
            {
                return NotFound();
            }

            return View(model);
        }


        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager, ProjectRole.Member)]
        public async Task<IActionResult> Details(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var model = await _projectService.GetDetailsViewModelAsync(projectId, userId);
            if (model is null)
            {
                return NotFound();
            }

            return View(model);
        }

        [ProjectAuthorize(ProjectRole.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProject(int projectId)
        {
            await _projectService.DeleteProjectAsync(projectId);

            return RedirectToAction("Index");
        }

        [ProjectAuthorize(ProjectRole.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProject(int projectId, EditProjectViewModel model)
        {
            if (projectId != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Challenge();
                }

                var detailsModel = await _projectService.GetDetailsViewModelAsync(projectId, userId);
                if (detailsModel is null)
                {
                    return NotFound();
                }

                detailsModel.EditProject = model;
                return View("Details", detailsModel);
            }

            var updated = await _projectService.EditProjectAsync(model);
            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { projectId });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(CreateProjectViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!ModelState.IsValid)
            {
                
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Challenge();
                }

                var indexModel = await _projectService.GetIndexViewModelAsync(userId);
                indexModel.CreateProject = model;
                return View("Index", indexModel);
            }

            
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            await _projectService.CreateProjectAsync(model, userId);


            return RedirectToAction("Index");
        }

        [ProjectAuthorize(ProjectRole.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserToProject(int projectId, string userId , ProjectRole role=ProjectRole.Member)
        {
            await _projectService.AddUserToProjectAsync(projectId, userId, role);
            return RedirectToAction("Details", new { projectId });
        }

        [ProjectAuthorize(ProjectRole.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRoleInProject(int projectId, string userId, ProjectRole newRole)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Challenge();
            }

            await _projectService.UpdateUserRoleInProjectAsync(projectId, userId, currentUserId, newRole);
            return RedirectToAction(nameof(Members), new { projectId });
        }

        [ProjectAuthorize(ProjectRole.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromProject(int projectId, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Challenge();
            }

            await _projectService.RemoveUserFromProjectAsync(projectId, userId, currentUserId);
            return RedirectToAction(nameof(Members), new { projectId });
        }
    }
}
