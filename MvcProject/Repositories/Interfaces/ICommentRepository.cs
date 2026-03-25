using MvcProject.Models.Domain;

namespace MvcProject.Repositories.Interfaces
{
    public interface ICommentRepository: IRepository<TaskComment>
    {
        Task<IEnumerable<TaskComment>> GetCommentsByTaskIdAsync(int taskId);
    }
}
