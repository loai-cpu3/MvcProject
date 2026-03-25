using Microsoft.AspNetCore.Mvc;

namespace MvcProject.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
