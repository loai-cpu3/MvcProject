namespace MvcProject.ViewModels.Projects
{
    public class AddProjectMembersViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public string SearchTerm { get; set; } = string.Empty;
        public List<AddProjectMemberUserViewModel> MatchedUsers { get; set; } = new List<AddProjectMemberUserViewModel>();
    }

    public class AddProjectMemberUserViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public bool AlreadyMember { get; set; }
    }
}
