using FitHub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitHub.Controllers
{
    public class UyeController : Controller
    {
        private readonly FitHubContext _context;

        public UyeController(FitHubContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Uye uye)
        {
            ModelState.Remove(nameof(Uye.Rol));

            if (!ModelState.IsValid)
            {
                return View(uye);
            }

            bool emailExists = await _context.Uyeler
                .AnyAsync(u => u.Email == uye.Email);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayýtlý.");
                return View(uye);
            }

            if (string.IsNullOrEmpty(uye.Rol))
            {
                uye.Rol = "Uye";
            }

            _context.Uyeler.Add(uye);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var uye = await _context.Uyeler
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Sifre == model.Sifre);

            if (uye == null)
            {
                ModelState.AddModelError("", "Geçersiz e-posta veya þifre.");
                return View(model);
            }

            // Set session values
            HttpContext.Session.SetInt32("UserId", uye.Id);
            HttpContext.Session.SetString("UserRole", uye.Rol ?? "");
            var adSoyad = (!string.IsNullOrWhiteSpace(uye.Ad) && !string.IsNullOrWhiteSpace(uye.Soyad))
                ? $"{uye.Ad} {uye.Soyad}"
                : uye.Email;
            HttpContext.Session.SetString("UserName", adSoyad);

            return RedirectToAction("Index", "Home");
        }

     

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
