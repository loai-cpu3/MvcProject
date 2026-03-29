using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Repositories.Interfaces;
using System.Security.Claims;

namespace MvcProject.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetUnread()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var notifications = await _unitOfWork.Notifications.GetUnreadByUserAsync(userId);

            var result = new List<object>();
            foreach (var n in notifications)
            {
                int projectId = 0;

                if (n.RelatedEntityType == "ProjectTask" && n.RelatedEntityId.HasValue)
                {
                    var task = await _unitOfWork.Tasks.GetByIdAsync(n.RelatedEntityId.Value);
                    projectId = task?.ProjectId ?? 0;
                }
                else if (n.RelatedEntityType == "Project" && n.RelatedEntityId.HasValue)
                {
                    projectId = n.RelatedEntityId.Value;
                }

                result.Add(new
                {
                    n.Id,
                    n.Content,
                    Type = n.Type.ToString(),
                    n.RelatedEntityType,
                    n.RelatedEntityId,
                    ProjectId = projectId,
                    SenderName = n.SenderUser != null
                        ? $"{n.SenderUser.FirstName} {n.SenderUser.LastName}"
                        : null,
                    n.CreatedAt
                });
            }

            return Ok(result);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var count = await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Verify the notification belongs to the current user
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
            if (notification == null) return NotFound();
            if (notification.UserId != userId) return Forbid();

            await _unitOfWork.Notifications.MarkAsReadAsync(id);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
            await _unitOfWork.SaveAsync();

            return Ok();
        }
    }
}
