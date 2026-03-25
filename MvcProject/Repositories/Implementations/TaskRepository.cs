<<<<<<< HEAD
=======
using Microsoft.EntityFrameworkCore;
using TaskStatus = MvcProject.Models.Enums.TaskStatus;

>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
namespace MvcProject.Repositories.Implementations
{
    public class TaskRepository: Repository<ProjectTask>, ITaskRepository
    {
        public TaskRepository(ApplicationDbContext context) : base(context)
        {
        }
<<<<<<< HEAD
=======

        public async Task<double> GetUserWeeklyCompletionRateAsync(int projectId, string userId)
        {
            var taskTotalAndCompleted = await _dbSet.Where(t => t.ProjectId == projectId && t.AssigneeId == userId)
                   .GroupBy(t => 1)
                   .Select(t=>  new {
                        Total = t.Count(),
                        Completed = t.Count(t=>t.Status == TaskStatus.Done),
                   }).FirstOrDefaultAsync();
            double rate = 0;

            if (taskTotalAndCompleted != null && taskTotalAndCompleted.Total > 0)
            {
                rate = taskTotalAndCompleted.Completed / (double)taskTotalAndCompleted.Total * 100;

            }
            return rate;

        }

        public async Task<int> GetTotalPendingTasksCountAsync()
        {
            return await _dbSet.CountAsync(t => t.Status != TaskStatus.Done);
        }

        public async Task<List<ProjectTask>> GetRecentTasksAsync(int count)
        {
            if (count <= 0)
            {
                return new List<ProjectTask>();
            }

            return await _dbSet
                .AsNoTracking()
                .Include(t => t.Project)
                .Where(t => t.Status != TaskStatus.Done)
                .OrderBy(t => t.Deadline ?? DateTime.MaxValue)
                .ThenByDescending(t => t.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
    }
}
