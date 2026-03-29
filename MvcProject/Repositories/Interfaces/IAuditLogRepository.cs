using MvcProject.Models.Domain;

namespace MvcProject.Repositories.Interfaces
{
    public interface IAuditLogRepository: IRepository<AuditLog>
    {
        Task<List<AuditLog>> GetRecentProjectActivitiesAsync(string userId, int count = 10);
    }
}
