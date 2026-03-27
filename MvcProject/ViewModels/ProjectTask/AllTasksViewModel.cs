using MvcProject.Models.Domain;

namespace MvcProject.ViewModels.ProjectTask
{
    public class AllTasksViewModel
    {
        public List<MvcProject.Models.Domain.ProjectTask> PastTasks { get; set; } = new();
        public List<MvcProject.Models.Domain.ProjectTask> PresentTasks { get; set; } = new();
        public string? SearchTerm { get; set; }
    }
}
