using Microsoft.EntityFrameworkCore;
using MvcProject.Data;
using MvcProject.Models.Domain;
using MvcProject.Repositories.Interfaces;

namespace MvcProject.Repositories.Implementations
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserAsync(string userId, int take = 20)
        {
            return await _context.Set<Notification>()
                .Include(n => n.SenderUser)
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Set<Notification>()
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(int id)
        {
            var notification = await _context.Set<Notification>().FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                _context.Set<Notification>().Update(notification);
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var unread = await _context.Set<Notification>()
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
            }

            _context.Set<Notification>().UpdateRange(unread);
        }
    }
}
