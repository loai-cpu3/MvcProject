using Microsoft.AspNetCore.Authorization;

namespace MvcProject.Authorization.Requirements
{
    public class ProjectRequirement : IAuthorizationRequirement
    {
        public ProjectRole TargetRole { get;}
        public ProjectRequirement(ProjectRole targetRole)
        {
            TargetRole = targetRole;
        }

    }
}
