using FitHub.Filters;
using FitHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitHub.Controllers
{
    // [AdminAuthorize] <-- BURADAKÝ KÝLÝDÝ KALDIRDIK
    public class SalonlarController : Controller
    {
        private readonly FitHubContext _context;

        public SalonlarController(FitHubContext context)
        {
            _context = context;
        }

        // HERKESE AÇIK (Salonlarýmýz Sayfasý)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Salonlar.ToListAsync());
        }

        // HERKESE AÇIK (Detay Sayfasý)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var salon = await _context.Salonlar
                .FirstOrDefaultAsync(m => m.Id == id);

            if (salon == null) return NotFound();

            return View(salon);
        }

        // --- SADECE ADMIN ÝÞLEMLERÝ ---

        [AdminAuthorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public async Task<IActionResult> Create(Salon salon)
        {
            if (ModelState.IsValid)
            {
                _context.Add(salon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(salon);
        }

        [AdminAuthorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var salon = await _context.Salonlar.FindAsync(id);
            if (salon == null) return NotFound();
            return View(salon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public async Task<IActionResult> Edit(int id, Salon salon)
        {
            if (id != salon.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(salon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(salon);
        }

        [AdminAuthorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var salon = await _context.Salonlar
                .FirstOrDefaultAsync(m => m.Id == id);
            if (salon == null) return NotFound();

            return View(salon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var salon = await _context.Salonlar.FindAsync(id);
            if (salon != null)
            {
                _context.Salonlar.Remove(salon);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}