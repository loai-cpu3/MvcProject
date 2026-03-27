using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MvcProject.Hubs;
using MvcProject.Models.Domain;
using MvcProject.Models.Enums;
using MvcProject.Repositories.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace MvcProject.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<CommentHub> _commentHub;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public CommentController(IUnitOfWork unitOfWork, IHubContext<CommentHub> commentHub, IHubContext<NotificationHub> notificationHub)
        {
            _unitOfWork = unitOfWork;
            _commentHub = commentHub;
            _notificationHub = notificationHub;
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetByTask(int taskId)
        {
            var comments = await _unitOfWork.Comments.GetCommentsByTaskIdAsync(taskId);
            
            var result = comments.Select(c => new
            {
                c.Id,
                c.Content,
                c.TaskId,
                UserId = c.UserId,
                AuthorName = c.User?.UserName ?? "Unknown",
                CreatedAt = c.CreatedAt
            });
            return Ok(result);
        }

        public class CreateCommentDto
        {
            public int TaskId { get; set; }
            public string Content { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var task = await _unitOfWork.Tasks.GetByIdAsync(dto.TaskId);
            if (task == null) return NotFound("Task not found");

            var comment = new TaskComment
            {
                TaskId = dto.TaskId,
                UserId = userId,
                Content = dto.Content,
                CreatedAt = System.DateTime.UtcNow
            };

            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.SaveAsync();

            // Fetch to ensure User navigation is populated for the notification
            var createdComments = await _unitOfWork.Comments.GetCommentsByTaskIdAsync(dto.TaskId);
            var createdComment = createdComments.FirstOrDefault(c => c.Id == comment.Id);

            // Notify via SignalR group using TaskId
            await _commentHub.Clients.Group(dto.TaskId.ToString()).SendAsync("ReceiveComment", new
            {
                comment.Id,
                comment.Content,
                comment.TaskId,
                AuthorName = createdComment?.User?.UserName ?? "Unknown",
                CreatedAt = comment.CreatedAt
            });

            // Create notification for the task assignee (if not the commenter)
            if (!string.IsNullOrEmpty(task.AssigneeId) && task.AssigneeId != userId)
            {
                var senderUser = await _unitOfWork.Users.GetByIdAsync(userId);
                var senderName = senderUser != null ? $"{senderUser.FirstName} {senderUser.LastName}" : "Someone";

                var notification = new Notification
                {
                    UserId = task.AssigneeId,
                    SenderUserId = userId,
                    Type = NotificationType.CommentAdded,
                    Content = $"{senderName} commented on \"{task.Title}\"",
                    RelatedEntityType = "ProjectTask",
                    RelatedEntityId = task.Id,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.SaveAsync();

                var unreadCount = await _unitOfWork.Notifications.GetUnreadCountAsync(task.AssigneeId);

                await _notificationHub.Clients.Group($"user_{task.AssigneeId}").SendAsync("ReceiveNotification", new
                {
                    notification.Id,
                    notification.Content,
                    Type = notification.Type.ToString(),
                    notification.RelatedEntityType,
                    notification.RelatedEntityId,
                    ProjectId = task.ProjectId,
                    SenderName = senderName,
                    notification.CreatedAt,
                    UnreadCount = unreadCount
                });
            }

            return Ok(new { comment.Id, comment.Content, comment.TaskId, AuthorName = createdComment?.User?.UserName ?? "Unknown", CreatedAt = comment.CreatedAt });
        }

        public class UpdateCommentDto
        {
            public string Content { get; set; }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var comment = await _unitOfWork.Comments.GetByIdAsync(id);
            if (comment == null) return NotFound();
            if (comment.UserId != userId) return Forbid(); // Only author can edit

            comment.Content = dto.Content;

            _unitOfWork.Comments.Update(comment);
            await _unitOfWork.SaveAsync();

            // Notify via SignalR
            await _commentHub.Clients.Group(comment.TaskId.ToString()).SendAsync("UpdateComment", new
            {
                comment.Id,
                comment.Content
            });

            return Ok(new { comment.Id, comment.Content });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var comment = await _unitOfWork.Comments.GetByIdAsync(id);
            if (comment == null) return NotFound();
            if (comment.UserId != userId) return Forbid(); // Only author can delete

            var taskId = comment.TaskId;

            _unitOfWork.Comments.Delete(comment);
            await _unitOfWork.SaveAsync();

            // Notify via SignalR
            await _commentHub.Clients.Group(taskId.ToString()).SendAsync("DeleteComment", new
            {
                Id = id
            });

            return Ok();
        }
    }
}
