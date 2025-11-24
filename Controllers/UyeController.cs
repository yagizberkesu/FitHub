// ...using statements...
using FitHub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

// ...namespace and class...

public class UyeController : Controller
{
    private readonly FitHubDBContext _context;

    public UyeController(FitHubDBContext context)
    {
        _context = context;
    }

    // GET: Uye/Register
    public IActionResult Register()
    {
        return View();
    }

    // POST: Uye/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingUser = await _context.Uyeler.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanýmda.");
            return View(model);
        }

        var uye = new Uye
        {
            Ad = model.Ad,
            Soyad = model.Soyad,
            Email = model.Email,
            Sifre = model.Sifre,
            Rol = "Uye"
        };

        _context.Uyeler.Add(uye);
        await _context.SaveChangesAsync();

        // Option: Redirect to Login after successful registration
        return RedirectToAction("Login", "Uye");
    }

    // GET: Uye/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: Uye/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Uyeler
            .FirstOrDefaultAsync(u => u.Email == model.Email && u.Sifre == model.Sifre);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "E-posta veya þifre hatalý.");
            return View(model);
        }

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserRole", user.Rol ?? "Uye");

        if (user.Rol == "Admin")
        {
            return RedirectToAction("AdminPanel", "Home");
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    // ...other actions...
}
