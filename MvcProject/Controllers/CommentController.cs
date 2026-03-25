using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MvcProject.Hubs;
using MvcProject.Models.Domain;
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

        public CommentController(IUnitOfWork unitOfWork, IHubContext<CommentHub> commentHub)
        {
            _unitOfWork = unitOfWork;
            _commentHub = commentHub;
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

            _unitOfWork.Comments.Delete(comment);
            await _unitOfWork.SaveAsync();

            return Ok();
        }
    }
}
