<<<<<<< HEAD
﻿namespace MvcProject.Repositories.Implementations
=======
﻿using Microsoft.EntityFrameworkCore;

namespace MvcProject.Repositories.Implementations
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        public ProjectRepository(ApplicationDbContext context) : base(context)
        {
        }
<<<<<<< HEAD
=======

        public async Task<int> GetTotalProjectsCountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<List<Project>> GetProjectsForUserAsync(string userId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.Tasks)
                .Where(p => p.Members.Any(m => m.UserId == userId))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Project?> GetProjectWithTasksAsync(int projectId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
    }
}
