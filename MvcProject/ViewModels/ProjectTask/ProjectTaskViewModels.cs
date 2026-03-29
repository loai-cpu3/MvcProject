using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MvcProject.Models.Enums;
using Microsoft.AspNetCore.Http;

namespace MvcProject.ViewModels.ProjectTask
{
    public class ProjectTaskCreateViewModel
    {
        public int ProjectId { get; set; }

        [Required, MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public Models.Enums.TaskStatus Status { get; set; } = Models.Enums.TaskStatus.ToDo;

        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime? Deadline { get; set; }

        public string? AssigneeId { get; set; }

        public IEnumerable<SelectListItem>? UsersList { get; set; }
        
        public IEnumerable<IFormFile>? Attachments { get; set; }
    }

    public class ProjectTaskEditViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }

        [Required, MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public Models.Enums.TaskStatus Status { get; set; }

        [Required]
        public TaskPriority Priority { get; set; }

        public DateTime? Deadline { get; set; }

        public string? AssigneeId { get; set; }

        public IEnumerable<SelectListItem>? UsersList { get; set; }
        
        public IEnumerable<IFormFile>? NewAttachments { get; set; }
        
        public List<AttachmentViewModel> ExistingAttachments { get; set; } = new List<AttachmentViewModel>();
    }

    public class ProjectTaskDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Models.Enums.TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? Deadline { get; set; }
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public string? AssigneeId { get; set; }
        public string AssigneeName { get; set; } = "Unassigned";
        public string? AssigneePhotoUrl { get; set; }
        public bool CanEditDelete { get; set; }
        public bool CanChangeStatus { get; set; }
        public List<AttachmentViewModel> Attachments { get; set; } = new List<AttachmentViewModel>();
        
        // Added for requirements compatibility
        public List<TaskCommentViewModel> Comments { get; set; } = new List<TaskCommentViewModel>();
    }

    public class AttachmentViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        
        // Helper for display
        public string SizeDisplay => Size < 1024 * 1024 
            ? $"{(Size / 1024d):F2} KB" 
            : $"{(Size / (1024d * 1024d)):F2} MB";
    }

    public class TaskCommentViewModel
    {
        public int Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorAvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Likes { get; set; }
        public List<TaskCommentViewModel> Replies { get; set; } = new List<TaskCommentViewModel>();
    }
    public static class TaskAttachmentConstants
    {
        public static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".png", ".jpg", ".jpeg", ".gif", ".webp", ".txt" };
        public static string AllowedExtensionsDisplay => string.Join(", ", AllowedExtensions).Replace(".", "").ToUpper();
        public static string AcceptAttribute => string.Join(",", AllowedExtensions);
    }
}
