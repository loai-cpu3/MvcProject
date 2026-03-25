namespace MvcProject.ViewModels.Home
{
    public class DashboardTaskViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string ProjectName { get; set; }

        public Models.Enums.TaskStatus Status { get; set; }

        public DateOnly DueDate { get; set; } = new DateOnly();

    }
}
