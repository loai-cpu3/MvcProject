namespace MvcProject.ViewModels.Home
{
    public class DashboardViewModel
    {
        public double CompletionRate { get; set; }
        public int TotalPendingTasks { get; set; }
        public int TotalProjects { get; set; }
        
        public List<DashboardTaskViewModel> RecentTasks { get; set; } = new List<DashboardTaskViewModel>();

    }
}
