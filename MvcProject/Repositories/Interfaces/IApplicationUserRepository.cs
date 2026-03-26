namespace MvcProject.Repositories.Interfaces
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        Task<List<ApplicationUser>> SearchByNameOrEmailAsync(string searchTerm, int maxResults);
    }
}
