using MvcProject.Models.Enums;
using TaskStatus = MvcProject.Models.Enums.TaskStatus;

namespace MvcProject.ViewModels.Projects
{
    public class ProjectTaskItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public string? AssigneeName { get; set; }
        public string? AssigneePhotoUrl { get; set; }
    }
}
