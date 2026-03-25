using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcProject.Attributes;
using MvcProject.Data;
using MvcProject.Models.Domain;
using MvcProject.Models.Enums;
using MvcProject.Services.Interfaces;
using MvcProject.ViewModels.ProjectTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MvcProject.Controllers
{
    public class ProjectTaskController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAttachmentService _attachmentService;

        public ProjectTaskController(ApplicationDbContext context, IAttachmentService attachmentService)
        {
            _context = context;
            _attachmentService = attachmentService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager, ProjectRole.Member)]
        public async Task<IActionResult> Detail(int id, int projectId)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.Assignee)
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(t => t.Id == id && t.ProjectId == projectId);

            if (task == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var projectUser = await _context.ProjectUsers
                .FirstOrDefaultAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

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
                ProjectTitle = task.Project.Title,
                AssigneeId = task.AssigneeId,
                AssigneeName = task.Assignee != null ? $"{task.Assignee.FirstName} {task.Assignee.LastName}" : "Unassigned",
                AssigneePhotoUrl = task.Assignee?.ProfilePictureUrl,
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

        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> Create(int projectId)
        {
            var project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return NotFound();

            var teamUserIds = project.Members.Select(tu => tu.UserId).ToList();
            var teamUsers = await _context.Users.Where(u => teamUserIds.Contains(u.Id)).ToListAsync();

            var model = new ProjectTaskCreateViewModel
            {
                ProjectId = projectId,
                UsersList = teamUsers.Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.UserName
                })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> Create(int projectId, ProjectTaskCreateViewModel model)
        {
            // Initial validation for files
            if (model.Attachments != null && model.Attachments.Any())
            {
                var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".png", ".jpg", ".jpeg", ".txt" };
                const long maxFileSize = 10 * 1024 * 1024; // 10 MB

                foreach (var file in model.Attachments)
                {
                    if (file.Length > maxFileSize)
                    {
                        ModelState.AddModelError("Attachments", $"File '{file.FileName}' exceeds the 10MB limit.");
                    }

                    var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("Attachments", $"File type for '{file.FileName}' is not allowed.");
                    }
                }
            }

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

                _context.ProjectTasks.Add(task);
                await _context.SaveChangesAsync();

                if (model.Attachments != null && model.Attachments.Any())
                {
                    foreach (var file in model.Attachments)
                    {
                        if (file.Length > 0)
                        {
                            try
                            {
                                await _attachmentService.UploadAsync(file, task.Id);
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError("Attachments", $"Error uploading '{file.FileName}': {ex.Message}");
                                // If upload fails after task creation, we might want to continue or rollback
                                // For now, we'll just report the error
                            }
                        }
                    }
                }

                // If we had attachment errors, we stay on the page to show them
                if (ModelState.IsValid)
                {
                    return RedirectToAction("Details", "Projects", new { projectId = model.ProjectId });
                }
            }

            // Repopulate UsersList on failure
            var project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == model.ProjectId);
            if (project != null)
            {
                var teamUserIds = project.Members.Select(tu => tu.UserId).ToList();
                var teamUsers = await _context.Users.Where(u => teamUserIds.Contains(u.Id)).ToListAsync();
                model.UsersList = teamUsers.Select(u => new SelectListItem { Value = u.Id, Text = u.UserName });
            }

            return View(model);
        }
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> Edit(int id, int projectId)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .ThenInclude(p => p.Members)
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(t => t.Id == id && t.ProjectId == projectId);

            if (task == null) return NotFound();

            var teamUserIds = task.Project.Members.Select(tu => tu.UserId).ToList();
            var teamUsers = await _context.Users.Where(u => teamUserIds.Contains(u.Id)).ToListAsync();

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
                UsersList = teamUsers.Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.UserName
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
        public async Task<IActionResult> Edit(int id, int projectId, ProjectTaskEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            // Initial validation for new files
            if (model.NewAttachments != null && model.NewAttachments.Any())
            {
                var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".png", ".jpg", ".jpeg", ".txt" };
                const long maxFileSize = 10 * 1024 * 1024; // 10 MB

                foreach (var file in model.NewAttachments)
                {
                    if (file.Length > maxFileSize)
                    {
                        ModelState.AddModelError("NewAttachments", $"File '{file.FileName}' exceeds the 10MB limit.");
                    }

                    var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("NewAttachments", $"File type for '{file.FileName}' is not allowed.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var task = await _context.ProjectTasks
                    .Include(t => t.Attachments)
                    .FirstOrDefaultAsync(t => t.Id == id && t.ProjectId == projectId);

                if (task == null) return NotFound();

                task.Title = model.Title;
                task.Description = model.Description;
                task.Status = model.Status;
                task.Priority = model.Priority;
                task.Deadline = model.Deadline;
                task.AssigneeId = model.AssigneeId;

                _context.ProjectTasks.Update(task);
                await _context.SaveChangesAsync();

                if (model.NewAttachments != null && model.NewAttachments.Any())
                {
                    foreach (var file in model.NewAttachments)
                    {
                        if (file.Length > 0)
                        {
                            try
                            {
                                await _attachmentService.UploadAsync(file, task.Id);
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError("NewAttachments", $"Error uploading '{file.FileName}': {ex.Message}");
                            }
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    return RedirectToAction("Detail", new { id = task.Id, projectId = task.ProjectId });
                }
            }

            // Repopulate UsersList and ExistingAttachments on failure
            var project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
            if (project != null)
            {
                var teamUserIds = project.Members.Select(tu => tu.UserId).ToList();
                var teamUsers = await _context.Users.Where(u => teamUserIds.Contains(u.Id)).ToListAsync();
                model.UsersList = teamUsers.Select(u => new SelectListItem { Value = u.Id, Text = u.UserName });
            }
            
            var existingTask = await _context.ProjectTasks.Include(t => t.Attachments).FirstOrDefaultAsync(t => t.Id == id);
            if (existingTask != null)
            {
                model.ExistingAttachments = existingTask.Attachments.Select(a => new AttachmentViewModel
                {
                    Id = a.Id,
                    FileName = a.OriginalFileName,
                    Size = a.Size,
                    ContentType = a.ContentType,
                    UploadedAt = a.UploadedAt
                }).ToList();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> DeleteAttachment(int id, int projectId, int taskId)
        {
            await _attachmentService.DeleteAsync(id);
            return RedirectToAction("Edit", new { id = taskId, projectId = projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public async Task<IActionResult> Delete(int id, int projectId)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(t => t.Id == id && t.ProjectId == projectId);

            if (task == null) return NotFound();

            // Delete attachments from disk and database
            if (task.Attachments != null && task.Attachments.Any())
            {
                foreach (var attachment in task.Attachments.ToList())
                {
                    await _attachmentService.DeleteAsync(attachment.Id);
                }
            }

            _context.ProjectTasks.Remove(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Projects", new { projectId = projectId });
        }


        
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public IActionResult Assign(int projectId, string userId)
        {
            return View();
        }


        
    }
}
