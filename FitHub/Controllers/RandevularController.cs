using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitHub.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;

namespace FitHub.Controllers
{
    public class RandevularController : Controller
    {
        private readonly FitHubContext _context;

        public RandevularController(FitHubContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "Admin";
        }

        // Dropdown listeleri doldurur
        private async Task LoadDropdowns()
        {
            ViewData["UyeId"] = new SelectList(await _context.Uyeler.ToListAsync(), "Id", "Ad");
            ViewData["EgitmenId"] = new SelectList(await _context.Egitmenler.ToListAsync(), "Id", "Ad");
            ViewData["SalonId"] = new SelectList(await _context.Salonlar.ToListAsync(), "Id", "Ad");
        }

        // GET: Randevular
        public async Task<IActionResult> Index()
        {
            var randevular = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Egitmen)
                .Include(r => r.Salon)
                .ToListAsync();
            return View(randevular);
        }

        // GET: Randevular/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Egitmen)
                .Include(r => r.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null) return NotFound();

            return View(randevu);
        }

        // GET: Randevular/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View();
        }

        // POST: Randevular/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Tarih,Saat,UyeId,EgitmenId,SalonId,Hizmet,Sure,Ucret,Onaylandi")] Randevu randevu)
        {
            var baslangic = randevu.Tarih.Date.Add(TimeSpan.Parse(randevu.Saat));
            var bitis = baslangic.AddMinutes(randevu.Sure);

            var conflicts = await _context.Randevular
                .Where(r =>
                    r.EgitmenId == randevu.EgitmenId &&
                    r.Durum != "Ýptal" &&
                    r.Tarih.Date == randevu.Tarih.Date &&
                    (
                        baslangic < r.Tarih.Date.Add(TimeSpan.Parse(r.Saat)).AddMinutes(r.Sure) &&
                        bitis > r.Tarih.Date.Add(TimeSpan.Parse(r.Saat))
                    )
                )
                .ToListAsync();

            if (conflicts.Any())
            {
                ModelState.AddModelError("", "Seçilen eðitmen bu saatte baþka bir randevuya sahip.");
                await LoadDropdowns();
                return View(randevu);
            }

            randevu.Durum = "Beklemede";
            _context.Add(randevu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Randevular/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            await LoadDropdowns();
            return View(randevu);
        }

        // POST: Randevular/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Tarih,Saat,UyeId,EgitmenId,SalonId,Hizmet,Sure,Ucret,Onaylandi,Durum")] Randevu randevu)
        {
            if (id != randevu.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(randevu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RandevuExists(randevu.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await LoadDropdowns();
            return View(randevu);
        }

        // GET: Randevular/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Egitmen)
                .Include(r => r.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null) return NotFound();

            return View(randevu);
        }

        // POST: Randevular/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                _context.Randevular.Remove(randevu);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool RandevuExists(int id)
        {
            return _context.Randevular.Any(e => e.Id == id);
        }
    }
}
