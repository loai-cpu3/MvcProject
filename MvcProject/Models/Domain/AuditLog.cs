using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public AuditActionType ActionType { get; set; }
        public string? Description { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
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
