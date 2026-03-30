using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
    public class ProjectUser : BaseEntity
    {
        public int ProjectId { get; set; }
        public string UserId { get; set; } = null!;

        [Required]
        public ProjectRole Role { get; set; }


        [ForeignKey("ProjectId")]
        public Project Project { get; set; } = null!;
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
    }
}
