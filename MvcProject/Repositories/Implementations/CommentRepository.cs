using Microsoft.EntityFrameworkCore;
using MvcProject.Data;
using MvcProject.Models.Domain;
using MvcProject.Repositories.Interfaces;

namespace MvcProject.Repositories.Implementations
{
    public class CommentRepository : Repository<TaskComment>, ICommentRepository
    {
        public CommentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TaskComment>> GetCommentsByTaskIdAsync(int taskId)
        {
            return await _context.Set<TaskComment>()
                .Include(c => c.User)
                .Where(c => c.TaskId == taskId)
                .OrderBy(c => c.CreatedAt) // Assuming BaseEntity has CreatedAt
                .ToListAsync();
        }
    }
}
