namespace MvcProject.ViewModels.Projects
{
    public class ProjectDetailsViewModel
    {
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int CompletionPercentage { get; set; }
        public EditProjectViewModel EditProject { get; set; } = new EditProjectViewModel();
    }
}
