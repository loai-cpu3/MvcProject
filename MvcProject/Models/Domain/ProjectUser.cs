using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
    public class ProjectUser
    {
        public int ProjectId { get; set; }
        public string UserId { get; set; }

        [Required]
        public ProjectRole Role { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;


        [ForeignKey("ProjectId")]
        public Project Project { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
