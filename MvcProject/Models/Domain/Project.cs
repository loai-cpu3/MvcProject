using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
    public class Project : BaseEntity
    {
        [Required, MaxLength(300)]
        public string Title { get; set; }
        public string? Description { get; set; }

        [Required]
        public string CreatedById { get; set; }


        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
        public ICollection<ProjectUser> Members { get; set; } = new List<ProjectUser>();



        [ForeignKey("CreatedById")]
        public ApplicationUser CreatedBy { get; set; }
    }
}
