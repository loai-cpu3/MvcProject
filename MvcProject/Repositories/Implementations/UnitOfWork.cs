namespace MvcProject.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        private IProjectRepository _projects;
        private ITaskRepository _tasks;
        private ICommentRepository _comments;
        private IAuditLogRepository _auditLogs;
        private IProjectUserRepository _projectUsers;
        private INotificationRepository _notifications;
        private IApplicationUserRepository _users;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IProjectRepository Projects
        {
            get
            {
                if (_projects == null)
                {
                    _projects = new ProjectRepository(_context);
                }
                return _projects;
            }
        }

        public ITaskRepository Tasks
        {
            get
            {
                if (_tasks == null)
                {
                    _tasks = new TaskRepository(_context);
                }
                return _tasks;
            }
        }

        public ICommentRepository Comments
        {
            get
            {
                if (_comments == null)
                {
                    _comments = new CommentRepository(_context);
                }
                return _comments;
            }
        }

        public IAuditLogRepository AuditLogs
        {
            get
            {
                if (_auditLogs == null)
                {
                    _auditLogs = new AuditLogRepository(_context);
                }
                return _auditLogs;
            }
        }

        public IProjectUserRepository ProjectUsers
        {
            get
            {
                if (_projectUsers == null)
                {
                    _projectUsers = new ProjectUserRepository(_context);
                }
                return _projectUsers;
            }
        }

        public INotificationRepository Notifications
        {
            get
            {
                if (_notifications == null)
                {
                    _notifications = new NotificationRepository(_context);
                }
                return _notifications;
            }
        }

        public IApplicationUserRepository Users
        {
            get
            {
                if (_users == null)
                {
                    _users = new ApplicationUserRepository(_context);
                }
                return _users;
            }
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

