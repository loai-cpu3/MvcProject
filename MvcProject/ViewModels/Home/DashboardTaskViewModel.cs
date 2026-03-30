namespace MvcProject.ViewModels.Home
{
    public class DashboardTaskViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;

        public Models.Enums.TaskStatus Status { get; set; }

        public DateOnly DueDate { get; set; } = new DateOnly();

    }
}
