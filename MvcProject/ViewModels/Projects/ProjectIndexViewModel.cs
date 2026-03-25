namespace MvcProject.ViewModels.Projects
{
    public class ProjectIndexViewModel
    {
        public List<ProjectIndexItemViewModel> Projects { get; set; } = new List<ProjectIndexItemViewModel>();
        public CreateProjectViewModel CreateProject { get; set; } = new CreateProjectViewModel();
    }

    public class ProjectIndexItemViewModel
    {
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CompletionPercentage { get; set; }
    }
}
