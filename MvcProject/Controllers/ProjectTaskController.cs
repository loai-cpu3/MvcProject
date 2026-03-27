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

        public async Task<IActionResult> MyTasks(int? projectId, MvcProject.Models.Enums.TaskStatus? status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            // Fetch all tasks in projects the user is a member of
            var tasks = await _unitOfWork.Tasks.GetAllTasksInUserProjectsAsync(userId);
            var userProjects = await _unitOfWork.Projects.GetProjectsForUserAsync(userId);

            // Apply filters
            if (projectId.HasValue)
            {
                tasks = tasks.Where(t => t.ProjectId == projectId.Value).ToList();
            }

            if (status.HasValue)
            {
                tasks = tasks.Where(t => t.Status == status.Value).ToList();
            }

            var model = new UserTasksViewModel
            {
                ToDoTasks = tasks.Where(t => t.Status == MvcProject.Models.Enums.TaskStatus.ToDo).ToList(),
                InProgressTasks = tasks.Where(t => t.Status == MvcProject.Models.Enums.TaskStatus.InProgress).ToList(),
                ReviewTasks = tasks.Where(t => t.Status == MvcProject.Models.Enums.TaskStatus.Review).ToList(),
                DoneTasks = tasks.Where(t => t.Status == MvcProject.Models.Enums.TaskStatus.Done).ToList(),
                
                SelectedProjectId = projectId,
                SelectedStatus = status,
                ProjectList = userProjects.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Title,
                    Selected = projectId == p.Id
                }).ToList(),
                StatusList = Enum.GetValues(typeof(MvcProject.Models.Enums.TaskStatus))
                    .Cast<MvcProject.Models.Enums.TaskStatus>()
                    .Select(s => new SelectListItem
                    {
                        Value = s.ToString(),
                        Text = s.ToString(),
                        Selected = status == s
                    }).ToList()
            };

            return View(model);
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }



        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager, ProjectRole.Member)]
        public async Task<IActionResult> Detail(int id, int projectId)
        {
            var task = await _unitOfWork.Tasks.GetByIdWithAttachmentsAsync(id);
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
        public async Task<IActionResult> CreateNewTask(int projectId, ProjectTaskCreateViewModel model)
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
                        var ext = Path.GetExtension(file.FileName).ToLower();
                        if (!TaskAttachmentConstants.AllowedExtensions.Contains(ext))
                        {
                            ModelState.AddModelError("Attachments", $"File type {ext} is not allowed. Allowed: {TaskAttachmentConstants.AllowedExtensionsDisplay}");
                            var m = await _unitOfWork.ProjectUsers.GetProjectMembersWithUsersAsync(model.ProjectId);
                            model.UsersList = m.Select(member => new SelectListItem { Value = member.UserId, Text = $"{member.User.FirstName} {member.User.LastName}" });
                            return View(model);
                        }
                    }

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
        public async Task<IActionResult> AjaxCreate(int projectId, ProjectTaskCreateViewModel model)
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
                        var ext = Path.GetExtension(file.FileName).ToLower();
                        if (!TaskAttachmentConstants.AllowedExtensions.Contains(ext))
                        {
                            return BadRequest($"File type {ext} is not allowed.");
                        }
                    }

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
            var task = await _unitOfWork.Tasks.GetByIdWithAttachmentsAsync(id);
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
                        var ext = Path.GetExtension(file.FileName).ToLower();
                        if (!TaskAttachmentConstants.AllowedExtensions.Contains(ext))
                        {
                            ModelState.AddModelError("NewAttachments", $"File type {ext} is not allowed. Allowed: {TaskAttachmentConstants.AllowedExtensionsDisplay}");
                            var m = await _unitOfWork.ProjectUsers.GetProjectMembersWithUsersAsync(model.ProjectId);
                            model.UsersList = m.Select(member => new SelectListItem { Value = member.UserId, Text = $"{member.User.FirstName} {member.User.LastName}" });
                            return View(model);
                        }
                    }

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
        [ValidateAntiForgeryToken]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager, ProjectRole.Member)]
        public async Task<IActionResult> UpdateStatus(int id, int projectId, Models.Enums.TaskStatus status)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(id);
            if (task == null || task.ProjectId != projectId) return NotFound();

            task.Status = status;
            _unitOfWork.Tasks.Update(task);
            await _unitOfWork.SaveAsync();

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
            var task = await _unitOfWork.Tasks.GetByIdWithAttachmentsAsync(id);
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
        public async Task<IActionResult> AjaxDelete(int id, int projectId)
        {
            var task = await _unitOfWork.Tasks.GetByIdWithAttachmentsAsync(id);
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

        [HttpPost]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> DeleteAllAttachments(int taskId, int projectId)
        {
            var task = await _unitOfWork.Tasks.GetByIdWithAttachmentsAsync(taskId);
            if (task == null || task.ProjectId != projectId) return NotFound();

            foreach (var attachment in task.Attachments.ToList())
            {
                await _attachmentService.DeleteAsync(attachment.Id);
            }

            return RedirectToAction("Edit", new { id = taskId, projectId = projectId });
        }
        public async Task<IActionResult> AllTasks(string searchTerm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var tasks = await _unitOfWork.Tasks.GetAllTasksInUserProjectsAsync(userId);

            // Filtering
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                tasks = tasks.Where(t => 
                    t.Project.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                    t.Status.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    t.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            var today = DateTime.Today;
            var model = new AllTasksViewModel
            {
                SearchTerm = searchTerm,
                PresentTasks = tasks.Where(t => t.Deadline == null || t.Deadline.Value.Date >= today)
                                    .OrderBy(t => t.Deadline ?? DateTime.MaxValue)
                                    .ToList(),
                PastTasks = tasks.Where(t => t.Deadline != null && t.Deadline.Value.Date < today)
                                 .OrderByDescending(t => t.Deadline)
                                 .ToList()
            };

            return View(model);
        }

    }
}
