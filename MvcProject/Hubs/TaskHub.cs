using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MvcProject.Hubs
{
    [Authorize]
    public class TaskHub : Hub
    {
    }
}
