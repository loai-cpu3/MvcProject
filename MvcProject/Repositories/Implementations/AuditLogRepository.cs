using Microsoft.EntityFrameworkCore;
using MvcProject.Models.Domain;
using MvcProject.Repositories.Interfaces;

namespace MvcProject.Repositories.Implementations
{
    public class AuditLogRepository: Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<AuditLog>> GetRecentProjectActivitiesAsync(string userId, int count = 10)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .Include(a => a.Task)
                    .ThenInclude(t => t.Project)
                .Where(a => _context.ProjectUsers.Any(pu => pu.ProjectId == a.Task.ProjectId && pu.UserId == userId))
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}
