using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MvcProject.Authorization.Requirements;
using System.Security.Claims;

namespace MvcProject.Authorization.Handlers
{
    public class ProjectRoleHandler :AuthorizationHandler<ProjectRequirement,int>
    {
        private readonly ApplicationDbContext _context;
        public ProjectRoleHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ProjectRequirement requirement,
            int projectId)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return;

            var userRole = await _context.ProjectUsers
                .AnyAsync(pu =>
                           pu.ProjectId == projectId &&
                           pu.UserId == userId &&
                           pu.Role == requirement.TargetRole
                );

            if (userRole)
            {
                context.Succeed(requirement);
            }

        }
      }
    }
