using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
<<<<<<< HEAD
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
=======
    public class AuditLog : BaseEntity
    {
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2

        [Required]
        public AuditActionType ActionType { get; set; }
        public string? Description { get; set; }
<<<<<<< HEAD
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
=======
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
        public string? UserId { get; set; }
        [Required]
        public int TaskId { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }


        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [ForeignKey("TaskId")]
        public ProjectTask Task { get; set; }
    }
}
