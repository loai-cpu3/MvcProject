using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using MvcProject.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TaskStatus = MvcProject.Models.Enums.TaskStatus;

namespace MvcProject.ViewModels.ProjectTask
{
    public class ProjectTaskEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(300, ErrorMessage = "Title cannot exceed 300 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public TaskStatus Status { get; set; }

        [Required(ErrorMessage = "Priority is required.")]
        public TaskPriority Priority { get; set; }

        [Required(ErrorMessage = "A deadline date is required.")]
        [DataType(DataType.Date)]
        public DateTime? Deadline { get; set; }

        [Required(ErrorMessage = "Project ID is required.")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Please select an assignee.")]
        [Display(Name = "Assignee")]
        public string? AssigneeId { get; set; }

        // Dropdown options for Assignee
        public IEnumerable<SelectListItem>? UsersList { get; set; }

        // Existing attachments for deletion
        public List<AttachmentViewModel> ExistingAttachments { get; set; } = new();

        // New file uploads
        [Display(Name = "Add New Attachments")]
        public List<IFormFile>? NewAttachments { get; set; }
    }
}
