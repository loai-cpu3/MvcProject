using MvcProject.Models.Enums;
using TaskStatus = MvcProject.Models.Enums.TaskStatus;

namespace MvcProject.ViewModels.ProjectTask
{
    public class ProjectTaskDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? Deadline { get; set; }
        public int ProjectId { get; set; }
        public string? ProjectTitle { get; set; }
        
        public string? AssigneeName { get; set; }
        public string? AssigneePhotoUrl { get; set; }
        public string? AssigneeId { get; set; }

        public bool CanEditDelete { get; set; }

        public List<AttachmentViewModel> Attachments { get; set; } = new();
    }

    public class AttachmentViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
}
