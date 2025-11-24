using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitHub.Models;
using Microsoft.AspNetCore.Http;

namespace FitHub.Controllers
{
    public class EgitmenlerController : Controller
    {
        private readonly FitHubContext _context;

        public EgitmenlerController(FitHubContext context)
        {
            _context = context;
        }

        // Helper method for admin check
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "Admin";
        }

        // GET: Egitmenler
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Uye");

            var egitmenler = await _context.Egitmenler.Include(e => e.Salon).ToListAsync();
            return View(egitmenler);
        }

        // GET: Egitmenler/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Uye");

            if (id == null) return NotFound();

            var egitmen = await _context.Egitmenler
                .Include(e => e.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (egitmen == null) return NotFound();

            return View(egitmen);
        }

        // GET: Egitmenler/Create
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Uye");

            ViewData["SalonId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Salonlar.ToListAsync(), "Id", "Ad");
            return View();
        }

        // POST: Egitmenler/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ad,UzmanlikAlanlari,Hizmetler,MusaitlikSaatleri,SalonId")] Egitmen egitmen)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Uye");

            if (ModelState.IsValid)
            {
                _context.Add(egitmen);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SalonId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Salonlar.ToListAsync(), "Id", "Ad", egitmen.SalonId);
            return View(egitmen);
        }

        // GET: Egitmenler/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Uye");

            if (id == null) return NotFound();

            var egitmen = await _context.Egitmenler.FindAsync(id);
            if (egitmen == null) return NotFound();

            ViewData["SalonId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Salonlar.ToListAsync(), "Id", "Ad", egitmen.SalonId);
            return View(egitmen);
        }

        // POST: Egitmenler/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,UzmanlikAlanlari,Hizmetler,MusaitlikSaatleri,SalonId")] Egitmen egitmen)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Uye");

            if (id != egitmen.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(egitmen);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EgitmenExists(egitmen.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SalonId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Salonlar.ToListAsync(), "Id", "Ad", egitmen.SalonId);
            return View(egitmen);
        }

        // GET: Egitmenler/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Uye");

            if (id == null) return NotFound();

            var egitmen = await _context.Egitmenler
                .Include(e => e.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (egitmen == null) return NotFound();

            return View(egitmen);
        }

        // POST: Egitmenler/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Uye");

            var egitmen = await _context.Egitmenler.FindAsync(id);
            if (egitmen != null)
            {
                _context.Egitmenler.Remove(egitmen);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EgitmenExists(int id)
        {
            return _context.Egitmenler.Any(e => e.Id == id);
        }
    }
}
