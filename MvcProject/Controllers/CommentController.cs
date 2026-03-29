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
                AuthorName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : "Unknown",
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

            var auditLog = new AuditLog
            {
                TaskId = task.Id,
                ActionType = AuditActionType.Update,
                UserId = userId,
                Description = "added a comment",
                CreatedAt = System.DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
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
                AuthorName = createdComment?.User != null ? $"{createdComment.User.FirstName} {createdComment.User.LastName}" : "Unknown",
                CreatedAt = comment.CreatedAt
            });

            // Notify all participants (assignee + anyone who commented) except the current commenter
            var participantIds = createdComments.Select(c => c.UserId).Distinct().ToList();
            if (!string.IsNullOrEmpty(task.AssigneeId))
            {
                participantIds.Add(task.AssigneeId);
            }

            var usersToNotify = participantIds.Where(id => id != userId).Distinct().ToList();

            if (usersToNotify.Any())
            {
                var senderUser = await _unitOfWork.Users.GetByIdAsync(userId);
                var senderName = senderUser != null ? $"{senderUser.FirstName} {senderUser.LastName}" : "Someone";

                foreach (var targetUserId in usersToNotify)
                {
                    var notification = new Notification
                    {
                        UserId = targetUserId,
                        SenderUserId = userId,
                        Type = NotificationType.CommentAdded,
                        Content = $"{senderName} commented on \"{task.Title}\"",
                        RelatedEntityType = "ProjectTask",
                        RelatedEntityId = task.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Notifications.AddAsync(notification);
                }
                
                await _unitOfWork.SaveAsync();

                // Send SignalR real-time updates for each notified user
                foreach (var targetUserId in usersToNotify)
                {
                    var unreadCount = await _unitOfWork.Notifications.GetUnreadCountAsync(targetUserId);

                    // We need to fetch the newly created notification's ID, but since we just saved them, 
                    // we can't easily map them back individually inside the loop (they all fired at once).
                    // So we will grab the last inserted notification for this user on this task.
                    var latestNotif = await _unitOfWork.Notifications.GetAllAsync();
                    var userNotif = latestNotif.OrderByDescending(n => n.Id)
                        .FirstOrDefault(n => n.UserId == targetUserId && n.RelatedEntityId == task.Id && n.Type == NotificationType.CommentAdded);

                    if (userNotif != null)
                    {
                        await _notificationHub.Clients.Group($"user_{targetUserId}").SendAsync("ReceiveNotification", new
                        {
                            userNotif.Id,
                            userNotif.Content,
                            Type = userNotif.Type.ToString(),
                            userNotif.RelatedEntityType,
                            userNotif.RelatedEntityId,
                            ProjectId = task.ProjectId,
                            SenderName = senderName,
                            userNotif.CreatedAt,
                            UnreadCount = unreadCount
                        });
                    }
                }
            }

            return Ok(new { comment.Id, comment.Content, comment.TaskId, AuthorName = createdComment?.User != null ? $"{createdComment.User.FirstName} {createdComment.User.LastName}" : "Unknown", CreatedAt = comment.CreatedAt });
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
