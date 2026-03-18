namespace MvcProject.Repositories.Implementations
{
    public class AuditLogRepository: Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
