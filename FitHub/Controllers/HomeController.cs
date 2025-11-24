using System.Diagnostics;
using FitHub.Models;
using Microsoft.AspNetCore.Mvc;

namespace FitHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
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
        // FitHub/Controllers/HomeController.cs
        public IActionResult AdminPanel()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return RedirectToAction("Login", "Uye");
            return View();
        }
    }
}
