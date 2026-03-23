using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
    public class TaskComment : BaseEntity
    {
        [Required]
        public string Content { get; set; }
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
