using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitHub.Models;
using Microsoft.AspNetCore.Http;

namespace FitHub.Controllers
{
    public class UyelerController : Controller
    {
        private readonly FitHubContext _context;

        public UyelerController(FitHubContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "Admin";
        }

        // GET: Uyeler
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Uye");

            var uyeler = await _context.Uyeler.ToListAsync();
            return View(uyeler);
        }

        // GET: Uyeler/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Uye");

            if (id == null) return NotFound();

            var uye = await _context.Uyeler.FirstOrDefaultAsync(m => m.Id == id);
            if (uye == null) return NotFound();

            return View(uye);
        }

        // GET: Uyeler/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Uye");

            if (id == null) return NotFound();

            var uye = await _context.Uyeler.FindAsync(id);
            if (uye == null) return NotFound();

            return View(uye);
        }

        // POST: Uyeler/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Soyad,Email,Sifre,Telefon,DogumTarihi,Cinsiyet,Rol")] Uye uye)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Uye");

            if (id != uye.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(uye);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UyeExists(uye.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(uye);
        }

        // GET: Uyeler/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Uye");

            if (id == null) return NotFound();

            var uye = await _context.Uyeler.FirstOrDefaultAsync(m => m.Id == id);
            if (uye == null) return NotFound();

            return View(uye);
        }

        // POST: Uyeler/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Uye");

            var uye = await _context.Uyeler.FindAsync(id);
            if (uye != null)
            {
                _context.Uyeler.Remove(uye);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UyeExists(int id)
        {
            return _context.Uyeler.Any(e => e.Id == id);
        }
    }
}
