using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
<<<<<<< HEAD
    public class ProjectUser
=======
    public class ProjectUser : BaseEntity
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
    {
        public int ProjectId { get; set; }
        public string UserId { get; set; }

        [Required]
        public ProjectRole Role { get; set; }
<<<<<<< HEAD
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
=======
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2


        [ForeignKey("ProjectId")]
        public Project Project { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
