using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcProject.Attributes;
using MvcProject.Models.Domain;
using MvcProject.Models.Enums;
using MvcProject.Services.Interfaces;
using MvcProject.ViewModels.ProjectTask;
using MvcProject.Repositories.Interfaces;
using System.Security.Claims;

namespace MvcProject.Controllers
{
    public class ProjectTaskController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAttachmentService _attachmentService;

        public ProjectTaskController(IUnitOfWork unitOfWork, IAttachmentService attachmentService)
        {
            _unitOfWork = unitOfWork;
            _attachmentService = attachmentService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }



        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager, ProjectRole.Member)]
        public async Task<IActionResult> Detail(int id, int projectId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(id);
            if (task == null || task.ProjectId != projectId) return NotFound();

            // Ensure related data is loaded. Since generic GetByIdAsync might not include them,
            // we might need a more specific method or handle it here if possible.
            // For now, let's assume we need to load them.

            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            var assignee = task.AssigneeId != null ? await _unitOfWork.Users.GetByIdAsync(task.AssigneeId) : null;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var projectUser = await _unitOfWork.ProjectUsers.GetByProjectAndUserAsync(projectId, userId);

            var canEditDelete = projectUser != null &&
                               (projectUser.Role == ProjectRole.Admin || projectUser.Role == ProjectRole.Manager);

            // Fetch attachments separately if they're not included
            var attachments = await _unitOfWork.Tasks.GetByIdAsync(id); // Simple way if it includes by default, otherwise we'd need a repo method

            var model = new ProjectTaskDetailViewModel
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
                Attachments = task.Attachments.Select(a => new AttachmentViewModel
                {
                    Id = a.Id,
                    FileName = a.OriginalFileName,
                    Size = a.Size,
                    ContentType = a.ContentType,
                    UploadedAt = a.UploadedAt
                }).ToList()
            };

            return View(model);
        }


        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager, ProjectRole.Member)]
        public async Task<IActionResult> DownloadAttachment(int id)
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
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project == null) return NotFound();

            var members = await _unitOfWork.ProjectUsers.GetProjectMembersWithUsersAsync(projectId);

            ProjectTaskCreateViewModel model = new ProjectTaskCreateViewModel
            {
                ProjectId = projectId,
                UsersList = members.Select(m => new SelectListItem
                {
                    Value = m.UserId,
                    Text = $"{m.User.FirstName} {m.User.LastName}"
                })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> CreateNewTask(ProjectTaskCreateViewModel model)
        {
            if (ModelState.IsValid)
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

            var members = await _unitOfWork.ProjectUsers.GetProjectMembersWithUsersAsync(model.ProjectId);
            model.UsersList = members.Select(m => new SelectListItem { Value = m.UserId, Text = $"{m.User.FirstName} {m.User.LastName}" });
            return View(model);
        }

        [HttpPost]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> AjaxCreate(ProjectTaskCreateViewModel model)
        {
            if (ModelState.IsValid)
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
            var task = await _unitOfWork.Tasks.GetByIdAsync(id);
            if (task == null || task.ProjectId != projectId) return NotFound();

            var members = await _unitOfWork.ProjectUsers.GetProjectMembersWithUsersAsync(projectId);

            var model = new ProjectTaskEditViewModel
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

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> Edit(ProjectTaskEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var task = await _unitOfWork.Tasks.GetByIdAsync(model.Id);
                if (task == null || task.ProjectId != model.ProjectId) return NotFound();

                task.Title = model.Title;
                task.Description = model.Description;
                task.Status = model.Status;
                task.Priority = model.Priority;
                task.Deadline = model.Deadline;
                task.AssigneeId = model.AssigneeId;

                _unitOfWork.Tasks.Update(task);
                await _unitOfWork.SaveAsync();

                if (model.NewAttachments != null && model.NewAttachments.Any())
                {
                    foreach (var file in model.NewAttachments)
                    {
                        if (file.Length > 0)
                        {
                            await _attachmentService.UploadAsync(file, task.Id);
                        }
                    }
                }

                return RedirectToAction("Detail", new { id = task.Id, projectId = task.ProjectId });
            }

            var members = await _unitOfWork.ProjectUsers.GetProjectMembersWithUsersAsync(model.ProjectId);
            model.UsersList = members.Select(m => new SelectListItem { Value = m.UserId, Text = $"{m.User.FirstName} {m.User.LastName}" });
            return View(model);
        }

        [HttpPost]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager, ProjectRole.Member)]
        public async Task<IActionResult> UpdateStatus(int id, Models.Enums.TaskStatus status)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(id);
            if (task == null) return NotFound();

            task.Status = status;
            _unitOfWork.Tasks.Update(task);
            await _unitOfWork.SaveAsync();

            return Json(new { success = true });
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
            var task = await _unitOfWork.Tasks.GetByIdAsync(id);
            if (task == null || task.ProjectId != projectId) return NotFound();

            // Delete attachments
            foreach (var attachment in task.Attachments.ToList())
            {
                await _attachmentService.DeleteAsync(attachment.Id);
            }

            _unitOfWork.Tasks.Delete(task);
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Details", "Projects", new { projectId = projectId });
        }

        [HttpPost]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> AjaxDelete(int id)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(id);
            if (task == null) return NotFound();

            // Delete attachments
            foreach (var attachment in task.Attachments.ToList())
            {
                await _attachmentService.DeleteAsync(attachment.Id);
            }

            _unitOfWork.Tasks.Delete(task);
            await _unitOfWork.SaveAsync();

            return Json(new { success = true });
        }
    }
}
