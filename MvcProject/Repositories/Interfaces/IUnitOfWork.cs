namespace MvcProject.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProjectRepository Projects { get; }
        ITaskRepository Tasks { get; }
        ICommentRepository Comments { get; }
        IAuditLogRepository AuditLogs { get; }
        IProjectUserRepository ProjectUsers { get; }
        INotificationRepository Notifications { get; }
        IApplicationUserRepository Users { get; }

        Task SaveAsync();
    }
}
