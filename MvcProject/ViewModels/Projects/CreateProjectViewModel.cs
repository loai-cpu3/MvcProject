using System.ComponentModel.DataAnnotations;

namespace MvcProject.ViewModels.Projects
{
    public class CreateProjectViewModel
    {
        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
