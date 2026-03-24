using MvcProject.Models.Domain;

namespace MvcProject.Repositories.Interfaces
{
    public interface IProjectRepository: IRepository<Project>
    {
        Task<int> GetTotalProjectsCountAsync();
    }
}
