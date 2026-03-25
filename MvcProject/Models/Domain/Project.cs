using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
<<<<<<< HEAD
    public class Project
    {
        [Key]
        public int Id { get; set; }
=======
    public class Project : BaseEntity
    {
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
        [Required, MaxLength(300)]
        public string Title { get; set; }
        public string? Description { get; set; }

<<<<<<< HEAD
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
=======
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
        [Required]
        public string CreatedById { get; set; }


        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
        public ICollection<ProjectUser> Members { get; set; } = new List<ProjectUser>();



        [ForeignKey("CreatedById")]
        public ApplicationUser CreatedBy { get; set; }
    }
}
