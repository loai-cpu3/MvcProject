namespace MvcProject.Repositories.Implementations
{
    public class ProjectUserRepository: Repository<ProjectUser>, IProjectUserRepository
    {
        public ProjectUserRepository(ApplicationDbContext context) : base(context)
        {
        }
       
    }
}
