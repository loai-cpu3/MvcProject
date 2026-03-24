using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Attributes;
using MvcProject.ViewModels.Projects;

namespace MvcProject.Controllers
{


    public class ProjectsController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }


        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager, ProjectRole.Member)]
        public IActionResult Details(int projectId)
        {
            return View();
        }

        [ProjectAuthorize(ProjectRole.Admin)]
        public IActionResult DeleteProject(int projectId)
        {
            return RedirectToAction("Index");
        }

        [ProjectAuthorize(ProjectRole.Admin)]
        public IActionResult EditProject(int projectId)
        {
            return View();
        }
        [ProjectAuthorize(ProjectRole.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateProject(CreateProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            return RedirectToAction("Index");
        }

        [ProjectAuthorize(ProjectRole.Admin)]
        public IActionResult AddUserToProject(int projectId, string userId , ProjectRole role=ProjectRole.Member)
        {
            return RedirectToAction("Details", new { projectId });
        }
         [ProjectAuthorize(ProjectRole.Admin)]
         public IActionResult UpdateUserRoleInProject(int projectId, string userId, ProjectRole newRole)
         {

             return RedirectToAction("Details", new { projectId });
         }
          [ProjectAuthorize(ProjectRole.Admin)]
          public IActionResult RemoveUserFromProject(int projectId, string userId)
          {
              return RedirectToAction("Details", new { projectId });
        }
    }
}
