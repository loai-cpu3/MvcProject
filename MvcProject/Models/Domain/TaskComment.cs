using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
    public class TaskComment : BaseEntity
    {
        [Required]
        public string Content { get; set; } = null!;
        [Required]
        public int TaskId { get; set; }
        [Required]
        public string UserId { get; set; } = null!;
 

        [ForeignKey("TaskId")]
        public ProjectTask Task { get; set; } = null!;
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
    }
}
