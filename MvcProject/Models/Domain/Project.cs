using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(300)]
        public string Title { get; set; }
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public string CreatedById { get; set; }


        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
        public ICollection<ProjectUser> Members { get; set; } = new List<ProjectUser>();



        [ForeignKey("CreatedById")]
        public ApplicationUser CreatedBy { get; set; }
    }
}
