using System.Diagnostics;
using FitHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Include iþlemleri için gerekli

namespace FitHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FitHubContext _context; // Veritabaný baðlamýný ekledik

        // Constructor'a context'i enjekte ediyoruz
        public HomeController(ILogger<HomeController> logger, FitHubContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Hizmetleri getirirken, iliþkili Eðitmenleri de (EgitmenHizmet tablosu üzerinden) dahil ediyoruz.
            var hizmetler = await _context.Hizmetler
                .Include(h => h.EgitmenHizmetler)
                .ThenInclude(eh => eh.Egitmen)
                .AsNoTracking()
                .ToListAsync();

            return View(hizmetler);
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

        public IActionResult AdminPanel()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return RedirectToAction("Login", "Uye");
            return View();
        }
    }
}