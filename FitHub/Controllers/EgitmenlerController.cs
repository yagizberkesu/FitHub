using FitHub.Filters;
using FitHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitHub.Controllers
{
    [AdminAuthorize]
    public class EgitmenlerController : Controller
    {
        private readonly FitHubContext _context;

        public EgitmenlerController(FitHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var egitmenler = await _context.Egitmenler
                .Include(e => e.Salon)
                .Include(e => e.EgitmenHizmetler)
                    .ThenInclude(eh => eh.Hizmet)
                .AsNoTracking()
                .ToListAsync();

            return View(egitmenler);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var egitmen = await _context.Egitmenler
                .Include(e => e.Salon)
                .Include(e => e.EgitmenHizmetler)
                    .ThenInclude(eh => eh.Hizmet)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (egitmen == null) return NotFound();
            return View(egitmen);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new EgitmenFormViewModel();
            await FillLists(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EgitmenFormViewModel vm)
        {
            await FillLists(vm);

            if (!ModelState.IsValid)
                return View(vm);

            var egitmen = new Egitmen
            {
                Ad = vm.Ad,
                UzmanlikAlanlari = vm.UzmanlikAlanlari ?? "",
                MusaitlikBaslangic = vm.MusaitlikBaslangic,
                MusaitlikBitis = vm.MusaitlikBitis,
                SalonId = vm.SalonId
            };

            _context.Egitmenler.Add(egitmen);
            await _context.SaveChangesAsync();

            // join kayıtları
            var selected = vm.SelectedHizmetIds?.Distinct().ToList() ?? new List<int>();
            foreach (var hid in selected)
                _context.EgitmenHizmetler.Add(new EgitmenHizmet { EgitmenId = egitmen.Id, HizmetId = hid });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var egitmen = await _context.Egitmenler
                .Include(e => e.EgitmenHizmetler)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (egitmen == null) return NotFound();

            var vm = new EgitmenFormViewModel
            {
                Id = egitmen.Id,
                Ad = egitmen.Ad,
                UzmanlikAlanlari = egitmen.UzmanlikAlanlari,
                MusaitlikBaslangic = egitmen.MusaitlikBaslangic,
                MusaitlikBitis = egitmen.MusaitlikBitis,
                SalonId = egitmen.SalonId,
                SelectedHizmetIds = egitmen.EgitmenHizmetler.Select(x => x.HizmetId).ToList()
            };

            await FillLists(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EgitmenFormViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            await FillLists(vm);

            if (!ModelState.IsValid)
                return View(vm);

            var egitmen = await _context.Egitmenler
                .Include(e => e.EgitmenHizmetler)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (egitmen == null) return NotFound();

            egitmen.Ad = vm.Ad;
            egitmen.UzmanlikAlanlari = vm.UzmanlikAlanlari ?? "";
            egitmen.MusaitlikBaslangic = vm.MusaitlikBaslangic;
            egitmen.MusaitlikBitis = vm.MusaitlikBitis;
            egitmen.SalonId = vm.SalonId;

            // join sync
            var selected = (vm.SelectedHizmetIds ?? new List<int>()).Distinct().ToList();

            var toRemove = egitmen.EgitmenHizmetler.Where(x => !selected.Contains(x.HizmetId)).ToList();
            _context.EgitmenHizmetler.RemoveRange(toRemove);

            var existingIds = egitmen.EgitmenHizmetler.Select(x => x.HizmetId).ToHashSet();
            var toAdd = selected.Where(x => !existingIds.Contains(x)).ToList();

            foreach (var hid in toAdd)
                _context.EgitmenHizmetler.Add(new EgitmenHizmet { EgitmenId = egitmen.Id, HizmetId = hid });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var egitmen = await _context.Egitmenler
                .Include(e => e.Salon)
                .Include(e => e.EgitmenHizmetler).ThenInclude(eh => eh.Hizmet)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (egitmen == null) return NotFound();
            return View(egitmen);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var egitmen = await _context.Egitmenler.FindAsync(id);
            if (egitmen != null)
            {
                _context.Egitmenler.Remove(egitmen);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task FillLists(EgitmenFormViewModel vm)
        {
            vm.Salonlar = await _context.Salonlar.AsNoTracking()
                .OrderBy(s => s.Ad)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Ad })
                .ToListAsync();

            // Hizmet listesi (şimdilik tüm hizmetler; istersen salon filtreleriz)
            vm.Hizmetler = await _context.Hizmetler.AsNoTracking()
                .OrderBy(h => h.Ad)
                .Select(h => new SelectListItem
                {
                    Value = h.Id.ToString(),
                    Text = $"{h.Ad} ({h.SureDakika} dk - {h.Ucret} ₺)"
                })
                .ToListAsync();
        }
    }
}
