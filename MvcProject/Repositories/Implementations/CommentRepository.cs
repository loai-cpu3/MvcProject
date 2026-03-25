<<<<<<< HEAD
﻿using MvcProject.Models.Domain;
=======
using Microsoft.EntityFrameworkCore;
using MvcProject.Data;
using MvcProject.Models.Domain;
using MvcProject.Repositories.Interfaces;
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2

namespace MvcProject.Repositories.Implementations
{
    public class CommentRepository : Repository<TaskComment>, ICommentRepository
    {
        public CommentRepository(ApplicationDbContext context) : base(context)
        {
        }
<<<<<<< HEAD
=======

        public async Task<IEnumerable<TaskComment>> GetCommentsByTaskIdAsync(int taskId)
        {
            return await _context.Set<TaskComment>()
                .Include(c => c.User)
                .Where(c => c.TaskId == taskId)
                .OrderBy(c => c.CreatedAt) // Assuming BaseEntity has CreatedAt
                .ToListAsync();
        }
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
    }
}
