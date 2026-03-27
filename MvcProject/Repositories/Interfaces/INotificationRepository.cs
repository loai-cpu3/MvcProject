using MvcProject.Models.Domain;

namespace MvcProject.Repositories.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUnreadByUserAsync(string userId, int take = 20);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int id);
        Task MarkAllAsReadAsync(string userId);
    }
}
