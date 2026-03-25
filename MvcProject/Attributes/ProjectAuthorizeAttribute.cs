using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MvcProject.Attributes
{
    public class ProjectAuthorizeAttribute : TypeFilterAttribute
    {
<<<<<<< HEAD
        public ProjectAuthorizeAttribute(ProjectRole Role) : base(typeof(ProjectAuthorizeFilter))
        {
            Arguments = new object[] { Role };
=======
        public ProjectAuthorizeAttribute(params ProjectRole[] Roles) : base(typeof(ProjectAuthorizeFilter))
        {
            Arguments = new object[] { Roles };
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
        }

    }

    public class ProjectAuthorizeFilter : IActionFilter { 
         
        private readonly IAuthorizationService _authorizationService;
<<<<<<< HEAD
        private readonly ProjectRole _targetRole;

        public ProjectAuthorizeFilter(IAuthorizationService authorizationService, ProjectRole targetRole)
        {
            _authorizationService = authorizationService;
            _targetRole = targetRole;
=======
        private readonly ProjectRole[] _targetRoles;

        public ProjectAuthorizeFilter(IAuthorizationService authorizationService, ProjectRole[] targetRoles)
        {
            _authorizationService = authorizationService;
            _targetRoles = targetRoles;
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
           
            if (!context.ActionArguments.TryGetValue("projectId", out var projectIdObj) || !(projectIdObj is int projectId))
            {
                context.Result = new BadRequestObjectResult("Project ID is required and must be an integer.");
                return;
            }
<<<<<<< HEAD
            
            string policyName = $"RequireProject{_targetRole}";

            var authorizationResult = _authorizationService.AuthorizeAsync(context.HttpContext.User, projectId, policyName).Result;
            if (!authorizationResult.Succeeded)
=======

            //string policyName = $"RequireProject{_targetRole}";

            AuthorizationResult result = null;
            foreach (var role in _targetRoles.Distinct())
            {
                var policyName = $"RequireProject{role}";
                result =  _authorizationService.AuthorizeAsync(context.HttpContext.User, projectId, policyName).Result;

                if (result.Succeeded)
                { 
                    return;
                }
            }

           
            if (result == null || !result.Succeeded)
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
            {
                context.Result = new ForbidResult();
            }
        }

       
    }

}
