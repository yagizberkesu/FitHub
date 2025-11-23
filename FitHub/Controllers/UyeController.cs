using FitHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class UyeController : Controller
{
    private readonly FitHubContext _context;
    public UyeController(FitHubContext context) { _context = context; }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(Uye uye)
    {
        if (ModelState.IsValid)
        {
            uye.Rol = "Uye"; // Varsayılan rol
            _context.Uyeler.Add(uye);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login");
        }
        return View(uye);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(Uye uye)
    {
        var user = await _context.Uyeler
            .FirstOrDefaultAsync(x => x.Email == uye.Email && x.Sifre == uye.Sifre);

        if (user != null)
        {
            // Basit oturum yönetimi (gerçek projede Identity kullanılır)
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Rol);

            // Admin yetkilendirmesi örneği
            if (user.Rol == "Admin")
                return RedirectToAction("Index", "Home");
            else
                return RedirectToAction("Index", "Home"); // Üye için de ana sayfa

            // Eğer farklı bir yönlendirme istiyorsan burada düzenleyebilirsin.
        }

        ModelState.AddModelError("", "Email veya şifre yanlış.");
        return View(uye);
    }
}
