using System.Threading.Tasks;
using FitHub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitHub.Filters;

namespace FitHub.Controllers
{
    // Tüm action'lar otomatik olarak /Uye/ActionName şeklinde olacak
    [Route("[controller]/[action]")]
    [AdminAuthorize]
    public class UyelerController : Controller
    {
        private readonly FitHubContext _context;

        public UyelerController(FitHubContext context)
        {
            _context = context;
        }

        // ===================== REGISTER =====================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Uye uye)
        {
            // Rol formdan gelmediği için validation'dan çıkarıyoruz
            ModelState.Remove(nameof(Uye.Rol));

            if (!ModelState.IsValid)
            {
                // Model valid değilse formu aynı modelle geri göster
                return View(uye);
            }

            // Aynı e-posta ile daha önce kayıt olmuş mu?
            bool emailExists = await _context.Uyeler
                .AnyAsync(u => u.Email == uye.Email);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
                return View(uye);
            }

            // Rol boşsa varsayılan "Uye" olsun
            if (string.IsNullOrEmpty(uye.Rol))
            {
                uye.Rol = "Uye";
            }

            _context.Uyeler.Add(uye);
            await _context.SaveChangesAsync();

            // Kayıt başarılı → Login sayfasına yönlendir
            return RedirectToAction(nameof(Login));
        }

        // ======================= LOGIN =======================

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
                // Sadece Email ve Sifre için hatalar gösterilecek
                return View(model);
            }

            var user = await _context.Uyeler
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Sifre == model.Sifre);

            if (user == null)
            {
                // Hatalı giriş → ekranda kırmızı hata mesajı
                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
                return View(model);
            }

            // Session’a yaz
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Rol ?? "Uye");

            // Admin ise admin panel, değilse ana sayfa
            if (user.Rol == "Admin")
            {
                return RedirectToAction("AdminPanel", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

        // ======================= LOGOUT ======================

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}
