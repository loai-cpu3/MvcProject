using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MvcProject.Attributes
{
    public class ProjectAuthorizeAttribute : TypeFilterAttribute
    {
        public ProjectAuthorizeAttribute(ProjectRole Role) : base(typeof(ProjectAuthorizeFilter))
        {
            Arguments = new object[] { Role };
        }

    }

    public class ProjectAuthorizeFilter : IActionFilter { 
         
        private readonly IAuthorizationService _authorizationService;
        private readonly ProjectRole _targetRole;

        public ProjectAuthorizeFilter(IAuthorizationService authorizationService, ProjectRole targetRole)
        {
            _authorizationService = authorizationService;
            _targetRole = targetRole;
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
            
            string policyName = $"RequireProject{_targetRole}";

            var authorizationResult = _authorizationService.AuthorizeAsync(context.HttpContext.User, projectId, policyName).Result;
            if (!authorizationResult.Succeeded)
            {
                context.Result = new ForbidResult();
            }
        }

       
    }

}
