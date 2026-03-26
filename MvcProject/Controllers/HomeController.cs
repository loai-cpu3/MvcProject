using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Services.Interfaces;
using MvcProject.ViewModels.Home;
using System.Diagnostics;

namespace MvcProject.Controllers
{
    public class HomeController : Controller
    {
        IDashboardService _dashboardService;
        public HomeController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
            //return RedirectToAction("login", "account");
        }

        public IActionResult Dashboard()
        {
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
