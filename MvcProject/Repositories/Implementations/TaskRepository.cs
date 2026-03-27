using Microsoft.EntityFrameworkCore;
using TaskStatus = MvcProject.Models.Enums.TaskStatus;

namespace MvcProject.Repositories.Implementations
{
    public class TaskRepository: Repository<ProjectTask>, ITaskRepository
    {
        public TaskRepository(ApplicationDbContext context) : base(context)
        {
        }

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

        public async Task<int> GetTotalPendingTasksCountAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
            }

            return await _dbSet.CountAsync(t =>
                t.Status != TaskStatus.Done &&
                t.Project.Members.Any(m => m.UserId == userId));
        }

        public async Task<List<ProjectTask>> GetRecentTasksAsync(string userId, int count)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
            }

            if (count <= 0)
            {
                return new List<ProjectTask>();
            }

            return await _dbSet
                .AsNoTracking()
                .Include(t => t.Project)
                .Where(t => t.Status != TaskStatus.Done && (t.AssigneeId == userId || t.AssigneeId == null))
                .OrderBy(t => t.Deadline ?? DateTime.MaxValue)
                .ThenByDescending(t => t.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<ProjectTask?> GetByIdWithAttachmentsAsync(int id)
        {
            return await _dbSet
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<ProjectTask>> GetAllUserTasksAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
            }

            return await _dbSet
                .AsNoTracking()
                .Include(t => t.Project)
                .Where(t => t.AssigneeId == userId)
                .OrderBy(t => t.Status)
                .ToListAsync();
        }

        public async Task<List<ProjectTask>> GetAllTasksInUserProjectsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
            }

            return await _dbSet
                .AsNoTracking()
                .Include(t => t.Project)
                .Where(t => t.Project.Members.Any(m => m.UserId == userId))
                .OrderBy(t => t.Status)
                .ThenBy(t => t.Deadline ?? DateTime.MaxValue)
                .ToListAsync();
        }
    }
}
