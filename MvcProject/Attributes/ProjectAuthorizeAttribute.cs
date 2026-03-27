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
            int? projectId = null;

            // 1. Try ActionArguments (matches action method parameter names)
            if (context.ActionArguments.TryGetValue("projectId", out var arg) && arg is int pId)
            {
                projectId = pId;
            }
            // 2. Try Route Data
            else if (context.RouteData.Values.TryGetValue("projectId", out var routeObj) && int.TryParse(routeObj.ToString(), out var rId))
            {
                projectId = rId;
            }
            // 3. Try Query string
            else if (context.HttpContext.Request.Query.TryGetValue("projectId", out var qIdStr) && int.TryParse(qIdStr, out var qId))
            {
                projectId = qId;
            }
            // 4. Try Form data
            else if (context.HttpContext.Request.HasFormContentType && context.HttpContext.Request.Form.TryGetValue("projectId", out var fIdStr) && int.TryParse(fIdStr, out var fId))
            {
                projectId = fId;
            }
            else if (context.HttpContext.Request.HasFormContentType && context.HttpContext.Request.Form.TryGetValue("ProjectId", out var fIdStr2) && int.TryParse(fIdStr2, out var fId2))
            {
                projectId = fId2;
            }
            // 5. Try to find ProjectId property in any model argument
            else
            {
                foreach (var argument in context.ActionArguments.Values)
                {
                    if (argument == null) continue;
                    var prop = argument.GetType().GetProperty("ProjectId");
                    if (prop != null && prop.PropertyType == typeof(int))
                    {
                        projectId = (int)prop.GetValue(argument);
                        break;
                    }
                }
            }

            if (!projectId.HasValue)
            {
                context.Result = new BadRequestObjectResult("Project ID is required and must be an integer.");
                return;
            }

            //string policyName = $"RequireProject{_targetRole}";

            AuthorizationResult result = null;
            foreach (var role in _targetRoles.Distinct())
            {
                var policyName = $"RequireProject{role}";
                result =  _authorizationService.AuthorizeAsync(context.HttpContext.User, projectId.Value, policyName).Result;

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
