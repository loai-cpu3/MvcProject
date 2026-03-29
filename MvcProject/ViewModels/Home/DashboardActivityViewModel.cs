namespace MvcProject.ViewModels.Home
{
    public class DashboardActivityViewModel
    {
        public int Id { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public string UserInitials { get; set; } = string.Empty;
        
        public string TaskTitle { get; set; } = string.Empty;
        public int TaskId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
