using System.ComponentModel.DataAnnotations;

namespace MvcProject.ViewModels.Projects
{
    public class EditProjectViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
