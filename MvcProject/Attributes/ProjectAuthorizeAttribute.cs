using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MvcProject.Attributes
{
    public class ProjectAuthorizeAttribute : TypeFilterAttribute
    {
        public ProjectAuthorizeAttribute(params ProjectRole[] Roles) : base(typeof(ProjectAuthorizeFilter))
        {
            Arguments = new object[] { Roles };
        }

    }

    public class ProjectAuthorizeFilter : IActionFilter { 
         
        private readonly IAuthorizationService _authorizationService;
        private readonly ProjectRole[] _targetRoles;

        public ProjectAuthorizeFilter(IAuthorizationService authorizationService, ProjectRole[] targetRoles)
        {
            _authorizationService = authorizationService;
            _targetRoles = targetRoles;
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
            {
                context.Result = new ForbidResult();
            }
        }

       
    }

}
