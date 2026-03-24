namespace MvcProject.Repositories.Implementations
{
    public class TaskRepository: Repository<ProjectTask>, ITaskRepository
    {
        public TaskRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
