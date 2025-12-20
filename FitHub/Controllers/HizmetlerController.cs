using FitHub.Filters;
using FitHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitHub.Controllers
{
    // [AdminAuthorize] BURADAN KALDIRDIK
    public class HizmetlerController : Controller
    {
        private readonly FitHubContext _context;

        public HizmetlerController(FitHubContext context)
        {
            _context = context;
        }

        // Herkese Açık
        public async Task<IActionResult> Index()
        {
            var list = await _context.Hizmetler
                .Include(h => h.Salon)
                .AsNoTracking()
                .OrderBy(h => h.Ad)
                .ToListAsync();

            return View(list);
        }

        // Herkese Açık
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var h = await _context.Hizmetler
                .Include(x => x.Salon)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (h == null) return NotFound();
            return View(h);
        }

        // Sadece Admin
        [AdminAuthorize]
        public async Task<IActionResult> Create()
        {
            ViewBag.SalonId = new SelectList(await _context.Salonlar.AsNoTracking().ToListAsync(), "Id", "Ad");
            return View(new Hizmet { SureDakika = 30, Ucret = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize] // Sadece Admin
        public async Task<IActionResult> Create(Hizmet hizmet)
        {
            if (hizmet.SureDakika % 30 != 0)
                ModelState.AddModelError(nameof(hizmet.SureDakika), "Süre 30 dakikanın katı olmalı.");

            if (!ModelState.IsValid)
            {
                ViewBag.SalonId = new SelectList(await _context.Salonlar.AsNoTracking().ToListAsync(), "Id", "Ad", hizmet.SalonId);
                return View(hizmet);
            }

            _context.Hizmetler.Add(hizmet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [AdminAuthorize] // Sadece Admin
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var h = await _context.Hizmetler.FindAsync(id);
            if (h == null) return NotFound();

            ViewBag.SalonId = new SelectList(await _context.Salonlar.AsNoTracking().ToListAsync(), "Id", "Ad", h.SalonId);
            return View(h);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize] // Sadece Admin
        public async Task<IActionResult> Edit(int id, Hizmet hizmet)
        {
            if (id != hizmet.Id) return NotFound();

            if (hizmet.SureDakika % 30 != 0)
                ModelState.AddModelError(nameof(hizmet.SureDakika), "Süre 30 dakikanın katı olmalı.");

            if (!ModelState.IsValid)
            {
                ViewBag.SalonId = new SelectList(await _context.Salonlar.AsNoTracking().ToListAsync(), "Id", "Ad", hizmet.SalonId);
                return View(hizmet);
            }

            _context.Update(hizmet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [AdminAuthorize] // Sadece Admin
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var h = await _context.Hizmetler
                .Include(x => x.Salon)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (h == null) return NotFound();
            return View(h);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminAuthorize] // Sadece Admin
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var h = await _context.Hizmetler.FindAsync(id);
            if (h != null)
            {
                _context.Hizmetler.Remove(h);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}