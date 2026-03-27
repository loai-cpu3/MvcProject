using MvcProject.Models.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MvcProject.ViewModels.ProjectTask
{
    public class UserTasksViewModel
    {
        public List<MvcProject.Models.Domain.ProjectTask> ToDoTasks { get; set; } = new();
        public List<MvcProject.Models.Domain.ProjectTask> InProgressTasks { get; set; } = new();
        public List<MvcProject.Models.Domain.ProjectTask> ReviewTasks { get; set; } = new();
        public List<MvcProject.Models.Domain.ProjectTask> DoneTasks { get; set; } = new();

        public MvcProject.Models.Enums.TaskStatus? SelectedStatus { get; set; }
        public int? SelectedProjectId { get; set; }
        public IEnumerable<SelectListItem> ProjectList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> StatusList { get; set; } = new List<SelectListItem>();
    }
}
