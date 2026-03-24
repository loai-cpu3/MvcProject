using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MvcProject.Hubs
{
    [Authorize]
    public class CommentHub : Hub
    {
        public async Task JoinTaskGroup(string taskId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, taskId);
        }

        public async Task LeaveTaskGroup(string taskId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, taskId);
        }
    }
}
