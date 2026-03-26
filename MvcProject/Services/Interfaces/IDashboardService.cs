using MvcProject.ViewModels.Home;

namespace MvcProject.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardViewModelAsync(string userId);
    }
}
