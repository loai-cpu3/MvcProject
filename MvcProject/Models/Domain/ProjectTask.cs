using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
<<<<<<< HEAD
    public class ProjectTask
    {
        [Key]
        public int Id { get; set; }
=======
    public class ProjectTask : BaseEntity
    {
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
        [Required, MaxLength(300)]
        public string Title { get; set; }
        public string? Description { get; set; }
        [Required]
        public Enums.TaskStatus Status { get; set; }
        [Required]
        public TaskPriority Priority { get; set; }
        public DateTime? Deadline { get; set; }
<<<<<<< HEAD
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
=======
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
        public int ProjectId { get; set; }
        public string? AssigneeId { get; set; }
  

        public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
<<<<<<< HEAD
=======
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2


        [ForeignKey("ProjectId")]
        public Project Project { get; set; }
        [ForeignKey("AssigneeId")]
<<<<<<< HEAD
        public ApplicationUser Assignee { get; set; }
=======
        public ApplicationUser? Assignee { get; set; }
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
    }
}
