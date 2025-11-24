using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FitHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace FitHub.Pages.Uyeler
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public LoginViewModel LoginViewModel { get; set; }

        private readonly FitHubContext _context;
        public LoginModel(FitHubContext context)
        {
            _context = context;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (LoginViewModel == null)
            {
                ModelState.AddModelError(string.Empty, "Form verileri eksik.");
                return Page();
            }

            var user = await _context.Uyeler
                .FirstOrDefaultAsync(u => u.Email == LoginViewModel.Email && u.Sifre == LoginViewModel.Sifre);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "E-posta veya þifre hatalý.");
                return Page();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Rol ?? "Uye");

            if (user.Rol == "Admin")
                return RedirectToPage("/AdminPanel");

            return RedirectToPage("/Index");
        }
    }
}
