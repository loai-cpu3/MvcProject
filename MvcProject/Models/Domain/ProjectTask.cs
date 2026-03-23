using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
    public class ProjectTask : BaseEntity
    {
        [Required, MaxLength(300)]
        public string Title { get; set; }
        public string? Description { get; set; }
        [Required]
        public Enums.TaskStatus Status { get; set; }
        [Required]
        public TaskPriority Priority { get; set; }
        public DateTime? Deadline { get; set; }
        public int ProjectId { get; set; }
        public string? AssigneeId { get; set; }
  

        public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();


        [ForeignKey("ProjectId")]
        public Project Project { get; set; }
        [ForeignKey("AssigneeId")]
        public ApplicationUser Assignee { get; set; }
    }
}
