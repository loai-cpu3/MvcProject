namespace MvcProject.ViewModels.Projects
{
    public class ProjectDetailsViewModel
    {
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsCurrentUserAdmin { get; set; }
        public bool IsCanCreateTask { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int CompletionPercentage { get; set; }
        public List<ProjectDetailsTaskItemViewModel> ToDoTasks { get; set; } = new List<ProjectDetailsTaskItemViewModel>();
        public List<ProjectDetailsTaskItemViewModel> InProgressTasks { get; set; } = new List<ProjectDetailsTaskItemViewModel>();
        public List<ProjectDetailsTaskItemViewModel> ReviewTasks { get; set; } = new List<ProjectDetailsTaskItemViewModel>();
        public List<ProjectDetailsTaskItemViewModel> DoneTasks { get; set; } = new List<ProjectDetailsTaskItemViewModel>();
        public List<ProjectMemberViewModel> Members { get; set; } = new List<ProjectMemberViewModel>();
        public EditProjectViewModel EditProject { get; set; } = new EditProjectViewModel();
    }

    public class ProjectDetailsTaskItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public string? AssigneeAvatarUrl { get; set; }
    }
    public class ProjectMemberViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
