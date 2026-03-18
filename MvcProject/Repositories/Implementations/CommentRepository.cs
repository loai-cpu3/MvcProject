using MvcProject.Models.Domain;

namespace MvcProject.Repositories.Implementations
{
    public class CommentRepository : Repository<TaskComment>, ICommentRepository
    {
        public CommentRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
