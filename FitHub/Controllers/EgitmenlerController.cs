using FitHub.Filters;
using FitHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitHub.Controllers
{
    [AdminAuthorize] // Bu controller altındaki tüm aksiyonlar sadece Admin yetkisi gerektirir.
    public class EgitmenlerController : Controller
    {
        private readonly FitHubContext _context;

        public EgitmenlerController(FitHubContext context)
        {
            _context = context;
        }

        // --- LİSTELEME (INDEX) ---
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

        // --- DETAYLAR (DETAILS) ---
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

        // --- YENİ KAYIT SAYFASI (CREATE - GET) ---
        public async Task<IActionResult> Create()
        {
            var vm = new EgitmenFormViewModel();
            await FillLists(vm); // Dropdown ve checkbox listelerini doldur
            return View(vm);
        }

        // --- YENİ KAYIT İŞLEMİ (CREATE - POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EgitmenFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await FillLists(vm); // Hata varsa listeleri tekrar doldur
                return View(vm);
            }

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

            // Seçilen hizmetleri EgitmenHizmet tablosuna ekle
            var selected = vm.SelectedHizmetIds?.Distinct().ToList() ?? new List<int>();
            foreach (var hid in selected)
            {
                _context.EgitmenHizmetler.Add(new EgitmenHizmet { EgitmenId = egitmen.Id, HizmetId = hid });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // --- DÜZENLEME SAYFASI (EDIT - GET) ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var egitmen = await _context.Egitmenler
                .Include(e => e.EgitmenHizmetler)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (egitmen == null) return NotFound();

            // Mevcut verileri ViewModel'e aktarıyoruz
            var vm = new EgitmenFormViewModel
            {
                Id = egitmen.Id,
                Ad = egitmen.Ad,
                UzmanlikAlanlari = egitmen.UzmanlikAlanlari,
                MusaitlikBaslangic = egitmen.MusaitlikBaslangic,
                MusaitlikBitis = egitmen.MusaitlikBitis,
                SalonId = egitmen.SalonId,
                // Mevcut seçili hizmetleri işaretlemek için listeyi alıyoruz
                SelectedHizmetIds = egitmen.EgitmenHizmetler.Select(x => x.HizmetId).ToList()
            };

            await FillLists(vm);
            return View(vm);
        }

        // --- DÜZENLEME İŞLEMİ (EDIT - POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EgitmenFormViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await FillLists(vm);
                return View(vm);
            }

            var egitmen = await _context.Egitmenler
                .Include(e => e.EgitmenHizmetler)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (egitmen == null) return NotFound();

            // Temel bilgileri güncelle
            egitmen.Ad = vm.Ad;
            egitmen.UzmanlikAlanlari = vm.UzmanlikAlanlari ?? "";
            egitmen.MusaitlikBaslangic = vm.MusaitlikBaslangic;
            egitmen.MusaitlikBitis = vm.MusaitlikBitis;
            egitmen.SalonId = vm.SalonId;

            // İlişkili Hizmetleri Güncelle (Sync)
            var selected = (vm.SelectedHizmetIds ?? new List<int>()).Distinct().ToList();

            // Silinecekler: Mevcut listede olup yeni seçimde olmayanlar
            var toRemove = egitmen.EgitmenHizmetler.Where(x => !selected.Contains(x.HizmetId)).ToList();
            _context.EgitmenHizmetler.RemoveRange(toRemove);

            // Eklenecekler: Yeni seçimde olup mevcut listede olmayanlar
            var existingIds = egitmen.EgitmenHizmetler.Select(x => x.HizmetId).ToHashSet();
            var toAdd = selected.Where(x => !existingIds.Contains(x)).ToList();

            foreach (var hid in toAdd)
            {
                _context.EgitmenHizmetler.Add(new EgitmenHizmet { EgitmenId = egitmen.Id, HizmetId = hid });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // --- SİLME SAYFASI (DELETE - GET) ---
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

        // --- SİLME İŞLEMİ (DELETE - POST) ---
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

        // --- YARDIMCI METOT: LİSTELERİ DOLDUR ---
        private async Task FillLists(EgitmenFormViewModel vm)
        {
            // Salon Listesi
            vm.Salonlar = await _context.Salonlar.AsNoTracking()
                .OrderBy(s => s.Ad)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Ad })
                .ToListAsync();

            // Hizmet Listesi (Checkboxlar için)
            var tumHizmetler = await _context.Hizmetler.AsNoTracking()
                .OrderBy(h => h.Ad)
                .ToListAsync();

            vm.Hizmetler = tumHizmetler.Select(h => new SelectListItem
            {
                Value = h.Id.ToString(),
                Text = $"{h.Ad} ({h.SureDakika} dk)",
                // Eğer ViewModel'deki seçili ID listesinde varsa işaretle
                Selected = vm.SelectedHizmetIds != null && vm.SelectedHizmetIds.Contains(h.Id)
            }).ToList();
        }
    }
}