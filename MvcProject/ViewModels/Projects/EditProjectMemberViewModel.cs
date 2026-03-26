using System.ComponentModel.DataAnnotations;

namespace MvcProject.ViewModels.Projects
{
    public class EditProjectMemberViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public ProjectRole CurrentRole { get; set; }

        [Required]
        public ProjectRole NewRole { get; set; }
    }
}
