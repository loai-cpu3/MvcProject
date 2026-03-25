using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
<<<<<<< HEAD
    public class TaskComment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
=======
    public class TaskComment : BaseEntity
    {
        [Required]
        public string Content { get; set; }
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
        [Required]
        public int TaskId { get; set; }
        [Required]
        public string UserId { get; set; }
 

        [ForeignKey("TaskId")]
        public ProjectTask Task { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
