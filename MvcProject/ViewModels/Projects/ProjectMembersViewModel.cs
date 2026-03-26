namespace MvcProject.ViewModels.Projects
{
    public class ProjectMembersViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public List<ProjectMemberCardViewModel> Members { get; set; } = new List<ProjectMemberCardViewModel>();
    }

    public class ProjectMemberCardViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public ProjectRole Role { get; set; }
    }
}
