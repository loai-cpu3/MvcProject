using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MvcProject.Attributes;
using MvcProject.Hubs;
using MvcProject.Models.Domain;
using MvcProject.Models.Enums;
using MvcProject.Services.Interfaces;
using MvcProject.ViewModels.ProjectTask;
using MvcProject.Repositories.Interfaces;
using System.Security.Claims;

namespace MvcProject.Controllers
{
    [Authorize]
    public class ProjectTaskController : Controller
    {
        private readonly IProjectTaskService _projectTaskService;
        private readonly IAttachmentService _attachmentService;

        public ProjectTaskController(IProjectTaskService projectTaskService, IAttachmentService attachmentService)
        {
            _projectTaskService = projectTaskService;
            _attachmentService = attachmentService;
        }

        public async Task<IActionResult> MyTasks(int? projectId, MvcProject.Models.Enums.TaskStatus? status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var model = await _projectTaskService.GetMyTasksViewModelAsync(userId, projectId, status);
            return View(model);
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager, ProjectRole.Member)]
        public async Task<IActionResult> Detail(int id, int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var model = await _projectTaskService.GetTaskDetailViewModelAsync(id, projectId, userId);
            if (model == null) return NotFound();

            return View(model);
        }

        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager, ProjectRole.Member)]
        public async Task<IActionResult> DownloadAttachment(int id, int projectId)
        {
            try
            {
                var (stream, contentType, fileName) = await _attachmentService.DownloadAsync(id);
                return File(stream, contentType, fileName);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> Create(int projectId)
        {
            var model = await _projectTaskService.GetCreateTaskViewModelAsync(projectId);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> CreateNewTask(int projectId, ProjectTaskCreateViewModel model)
        {
            var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(actorId)) return Challenge();

            if (ModelState.IsValid)
            {
                if (model.Attachments != null && model.Attachments.Any())
                {
                    foreach (var file in model.Attachments)
                    {
                        var ext = Path.GetExtension(file.FileName).ToLower();
                        if (!TaskAttachmentConstants.AllowedExtensions.Contains(ext))
                        {
                            ModelState.AddModelError("Attachments", $"File type {ext} is not allowed. Allowed: {TaskAttachmentConstants.AllowedExtensionsDisplay}");
                            var m = await _projectTaskService.GetCreateTaskViewModelAsync(model.ProjectId);
                            model.UsersList = m?.UsersList ?? new List<SelectListItem>();
                            return View("Create", model);
                        }
                    }
                }

                var task = await _projectTaskService.CreateTaskAsync(model, actorId);

                if (model.Attachments != null && model.Attachments.Any())
                {
                    foreach (var file in model.Attachments)
                    {
                        if (file.Length > 0)
                        {
                            await _attachmentService.UploadAsync(file, task.Id);
                        }
                    }
                }

                return RedirectToAction("Details", "Projects", new { projectId = model.ProjectId });
            }

            var fallbackModel = await _projectTaskService.GetCreateTaskViewModelAsync(model.ProjectId);
            model.UsersList = fallbackModel?.UsersList ?? new List<SelectListItem>();
            return View("Create", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> AjaxCreate(int projectId, ProjectTaskCreateViewModel model)
        {
            var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(actorId)) return Challenge();

            if (ModelState.IsValid)
            {
                if (model.Attachments != null && model.Attachments.Any())
                {
                    foreach (var file in model.Attachments)
                    {
                        var ext = Path.GetExtension(file.FileName).ToLower();
                        if (!TaskAttachmentConstants.AllowedExtensions.Contains(ext))
                        {
                            return BadRequest($"File type {ext} is not allowed.");
                        }
                    }
                }

                var task = await _projectTaskService.CreateTaskAsync(model, actorId);

                if (model.Attachments != null && model.Attachments.Any())
                {
                    foreach (var file in model.Attachments)
                    {
                        if (file.Length > 0)
                        {
                            await _attachmentService.UploadAsync(file, task.Id);
                        }
                    }
                }

                return Json(new { success = true });
            }
            return BadRequest(ModelState);
        }

        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> Edit(int id, int projectId)
        {
            var model = await _projectTaskService.GetEditTaskViewModelAsync(id, projectId);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> Edit(ProjectTaskEditViewModel model)
        {
            var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(actorId)) return Challenge();

            if (ModelState.IsValid)
            {
                if (model.NewAttachments != null && model.NewAttachments.Any())
                {
                    foreach (var file in model.NewAttachments)
                    {
                        var ext = Path.GetExtension(file.FileName).ToLower();
                        if (!TaskAttachmentConstants.AllowedExtensions.Contains(ext))
                        {
                            ModelState.AddModelError("NewAttachments", $"File type {ext} is not allowed. Allowed: {TaskAttachmentConstants.AllowedExtensionsDisplay}");
                            var m = await _projectTaskService.GetEditTaskViewModelAsync(model.Id, model.ProjectId);
                            model.UsersList = m?.UsersList ?? new List<SelectListItem>();
                            return View(model);
                        }
                    }
                }

                var updated = await _projectTaskService.UpdateTaskAsync(model, actorId);
                if (!updated) return NotFound();

                if (model.NewAttachments != null && model.NewAttachments.Any())
                {
                    foreach (var file in model.NewAttachments)
                    {
                        if (file.Length > 0)
                        {
                            await _attachmentService.UploadAsync(file, model.Id);
                        }
                    }
                }

                return RedirectToAction("Detail", new { id = model.Id, projectId = model.ProjectId });
            }

            var fallbackModel = await _projectTaskService.GetEditTaskViewModelAsync(model.Id, model.ProjectId);
            model.UsersList = fallbackModel?.UsersList ?? new List<SelectListItem>();
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager, ProjectRole.Member)]
        public async Task<IActionResult> UpdateStatus(int id, int projectId, Models.Enums.TaskStatus status)
        {
            var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(actorId)) return Challenge();

            var result = await _projectTaskService.UpdateTaskStatusAsync(id, projectId, status, actorId);
            if (!result) return NotFound();

            return RedirectToAction("Details", "Projects", new { projectId = projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> DeleteAttachment(int id, int taskId, int projectId)
        {
            await _attachmentService.DeleteAsync(id);
            return RedirectToAction("Edit", new { id = taskId, projectId = projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> Delete(int id, int projectId)
        {
            var model = await _projectTaskService.GetEditTaskViewModelAsync(id, projectId);
            if (model == null) return NotFound();

            if (model.ExistingAttachments != null)
            {
                foreach (var attachment in model.ExistingAttachments)
                {
                    await _attachmentService.DeleteAsync(attachment.Id);
                }
            }

            await _projectTaskService.DeleteTaskAsync(id, projectId);

            return RedirectToAction("Details", "Projects", new { projectId = projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> AjaxDelete(int id, int projectId)
        {
            var model = await _projectTaskService.GetEditTaskViewModelAsync(id, projectId);
            if (model == null) return NotFound();

            if (model.ExistingAttachments != null)
            {
                foreach (var attachment in model.ExistingAttachments)
                {
                    await _attachmentService.DeleteAsync(attachment.Id);
                }
            }

            await _projectTaskService.DeleteTaskAsync(id, projectId);

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> DeleteAllAttachments(int taskId, int projectId)
        {
            var model = await _projectTaskService.GetEditTaskViewModelAsync(taskId, projectId);
            if (model == null) return NotFound();

            if (model.ExistingAttachments != null)
            {
                foreach (var attachment in model.ExistingAttachments)
                {
                    await _attachmentService.DeleteAsync(attachment.Id);
                }
            }

            return RedirectToAction("Edit", new { id = taskId, projectId = projectId });
        }
        
        public async Task<IActionResult> AllTasks(string searchTerm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var model = await _projectTaskService.GetAllTasksViewModelAsync(userId, searchTerm);
            return View(model);
        }
    }
}
