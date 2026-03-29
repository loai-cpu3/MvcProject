using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Repositories.Interfaces;
using System.Security.Claims;

namespace MvcProject.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public TeamController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            // Get all projects the user is a member of
            var projects = await _unitOfWork.Projects.GetProjectsForUserAsync(userId);

            // Collect unique team members across all shared projects
            var teamMembers = new Dictionary<string, TeamMemberViewModel>();

            foreach (var project in projects)
            {
                var members = await _unitOfWork.ProjectUsers.GetProjectMembersWithUsersAsync(project.Id);
                foreach (var member in members)
                {
                    if (!teamMembers.ContainsKey(member.UserId))
                    {
                        teamMembers[member.UserId] = new TeamMemberViewModel
                        {
                            UserId = member.UserId,
                            FullName = $"{member.User.FirstName} {member.User.LastName}".Trim(),
                            Email = member.User.Email,
                            AvatarUrl = member.User.ProfilePictureUrl,
                            Role = member.Role,
                            ProjectNames = new List<string> { project.Title }
                        };
                    }
                    else
                    {
                        teamMembers[member.UserId].ProjectNames.Add(project.Title);
                        // Use the highest role across projects
                        if (member.Role < teamMembers[member.UserId].Role)
                        {
                            teamMembers[member.UserId].Role = member.Role;
                        }
                    }
                }
            }

            var model = new TeamIndexViewModel
            {
                Members = teamMembers.Values
                    .OrderBy(m => m.Role)
                    .ThenBy(m => m.FullName)
                    .ToList(),
                CurrentUserId = userId
            };

            return View(model);
        }
    }

    public class TeamMemberViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public ProjectRole Role { get; set; }
        public List<string> ProjectNames { get; set; } = new();
    }

    public class TeamIndexViewModel
    {
        public List<TeamMemberViewModel> Members { get; set; } = new();
        public string CurrentUserId { get; set; } = string.Empty;
    }
}
