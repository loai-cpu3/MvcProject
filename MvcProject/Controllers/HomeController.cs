using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
<<<<<<< HEAD
=======
using MvcProject.Services.Interfaces;
using MvcProject.ViewModels.Home;
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
using System.Diagnostics;

namespace MvcProject.Controllers
{
    public class HomeController : Controller
    {
<<<<<<< HEAD
=======
        IDashboardService _dashboardService;
        public HomeController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        public IActionResult Dashboard()
        {
<<<<<<< HEAD
            return View();
=======
            return View(new DashboardViewModel() { 
                    CompletionRate = 75.5,
                    TotalPendingTasks = 12,
                    TotalProjects = 5,
                    RecentTasks = new List<DashboardTaskViewModel>()
                    {
                        new DashboardTaskViewModel() { Id = 1, Title = "Design Database Schema", ProjectName = "Project Alpha", DueDate =DateOnly.FromDateTime( DateTime.UtcNow.AddDays(3)) },
                        new DashboardTaskViewModel() { Id = 2, Title = "Implement Authentication", ProjectName = "Project Beta", DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)) },
                        new DashboardTaskViewModel() { Id = 3, Title = "Create API Endpoints", ProjectName = "Project Gamma", DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)) }
                    }
            });
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
