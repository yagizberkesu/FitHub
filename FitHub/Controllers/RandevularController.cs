using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitHub.Filters;
using FitHub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitHub.Controllers
{
    // Üye randevu alýr, admin onaylar/iptal eder.
    public class RandevularController : Controller
    {
        private readonly FitHubContext _context;

        public RandevularController(FitHubContext context)
        {
            _context = context;
        }

        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");
        private string CurrentRole => HttpContext.Session.GetString("UserRole") ?? string.Empty;
        private bool IsAdmin => string.Equals(CurrentRole, "Admin", StringComparison.OrdinalIgnoreCase);

        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == null) return RedirectToAction("Login", "Uye");

            IQueryable<Randevu> q = _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Egitmen)
                .Include(r => r.Salon)
                .AsNoTracking();

            if (!IsAdmin)
                q = q.Where(r => r.UyeId == CurrentUserId.Value);

            var list = await q
                .OrderByDescending(r => r.Tarih)
                .ThenByDescending(r => r.Baslangic)
                .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            if (CurrentUserId == null) return RedirectToAction("Login", "Uye");

            var vm = new RandevuCreateViewModel();
            await FillDropDowns(vm);

            if (vm.Salonlar.Count > 0) vm.SalonId = int.Parse(vm.Salonlar[0].Value!);
            if (vm.Egitmenler.Count > 0) vm.EgitmenId = int.Parse(vm.Egitmenler[0].Value!);

            vm.SaatSecenekleri = await ComputeAvailableSlots(vm.SalonId, vm.EgitmenId, vm.Tarih, vm.Sure, vm.KisiSayisi);
            if (vm.SaatSecenekleri.Count > 0) vm.BaslangicStr = vm.SaatSecenekleri[0];

            return View(vm);
        }

        // JS burayý çaðýrýyor (salon/egitmen/tarih/sure/kisi deðiþince)
        [HttpGet]
        public async Task<IActionResult> GetSlots(int salonId, int egitmenId, string tarih, int sure, int kisiSayisi)
        {
            if (CurrentUserId == null) return Unauthorized();

            if (!DateOnly.TryParse(tarih, out var date))
                return BadRequest("tarih formatý yanlýþ (yyyy-MM-dd)");

            if (sure % 30 != 0 || sure <= 0)
                return BadRequest("sure 30'un katý olmalý");

            if (kisiSayisi <= 0) kisiSayisi = 1;

            var slots = await ComputeAvailableSlots(salonId, egitmenId, date, sure, kisiSayisi);
            return Json(slots);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RandevuCreateViewModel vm)
        {
            if (CurrentUserId == null) return RedirectToAction("Login", "Uye");

            await FillDropDowns(vm);

            if (vm.Sure % 30 != 0)
                ModelState.AddModelError(nameof(vm.Sure), "Süre 30 dakikanýn katý olmalý.");

            if (!TimeOnly.TryParseExact(vm.BaslangicStr, "HH:mm", out var baslangic))
                ModelState.AddModelError(nameof(vm.BaslangicStr), "Geçersiz saat seçimi.");

            if (!ModelState.IsValid)
            {
                vm.SaatSecenekleri = await ComputeAvailableSlots(vm.SalonId, vm.EgitmenId, vm.Tarih, vm.Sure, vm.KisiSayisi);
                return View(vm);
            }

            var bitis = baslangic.AddMinutes(vm.Sure);

            // “Seçilen slot hala uygun mu?” kontrolü (race-condition engeli)
            var available = await ComputeAvailableSlots(vm.SalonId, vm.EgitmenId, vm.Tarih, vm.Sure, vm.KisiSayisi);
            if (!available.Contains(vm.BaslangicStr))
            {
                ModelState.AddModelError("", "Seçtiðiniz saat artýk uygun deðil. Lütfen tekrar seçin.");
                vm.SaatSecenekleri = available;
                return View(vm);
            }

            var entity = new Randevu
            {
                SalonId = vm.SalonId,
                EgitmenId = vm.EgitmenId,
                UyeId = CurrentUserId.Value,
                Tarih = vm.Tarih,
                Baslangic = baslangic,
                Bitis = bitis,
                Sure = vm.Sure,
                KisiSayisi = vm.KisiSayisi,
                Hizmet = vm.Hizmet ?? string.Empty,
                Ucret = vm.Ucret,
                Durum = "Beklemede",
                Onaylandi = false
            };

            _context.Randevular.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (CurrentUserId == null) return RedirectToAction("Login", "Uye");
            if (id == null) return NotFound();

            var r = await _context.Randevular
                .Include(x => x.Uye)
                .Include(x => x.Egitmen)
                .Include(x => x.Salon)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (r == null) return NotFound();
            if (!IsAdmin && r.UyeId != CurrentUserId.Value) return Forbid();

            return View(r);
        }

        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var r = await _context.Randevular.FindAsync(id);
            if (r == null) return NotFound();

            r.Durum = "Onaylandý";
            r.Onaylandi = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var r = await _context.Randevular.FindAsync(id);
            if (r == null) return NotFound();

            r.Durum = "Ýptal";
            r.Onaylandi = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [AdminAuthorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var r = await _context.Randevular
                .Include(x => x.Uye)
                .Include(x => x.Egitmen)
                .Include(x => x.Salon)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (r == null) return NotFound();
            return View(r);
        }

        [AdminAuthorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var r = await _context.Randevular.FindAsync(id);
            if (r != null)
            {
                _context.Randevular.Remove(r);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task FillDropDowns(RandevuCreateViewModel vm)
        {
            vm.Salonlar = await _context.Salonlar
                .AsNoTracking()
                .OrderBy(s => s.Ad)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Ad })
                .ToListAsync();

            vm.Egitmenler = await _context.Egitmenler
                .AsNoTracking()
                .OrderBy(e => e.Ad)
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Ad })
                .ToListAsync();
        }

        private static bool Overlaps(TimeOnly aStart, TimeOnly aEnd, TimeOnly bStart, TimeOnly bEnd)
            => aStart < bEnd && aEnd > bStart;

        private async Task<List<string>> ComputeAvailableSlots(int salonId, int egitmenId, DateOnly tarih, int sure, int kisiSayisi)
        {
            var salon = await _context.Salonlar.AsNoTracking().FirstOrDefaultAsync(s => s.Id == salonId);
            var egitmen = await _context.Egitmenler.AsNoTracking().FirstOrDefaultAsync(e => e.Id == egitmenId);

            if (salon == null || egitmen == null) return new List<string>();

            // salon + eðitmen saatlerinin kesiþimi
            var start = salon.CalismaBaslangic > egitmen.MusaitlikBaslangic ? salon.CalismaBaslangic : egitmen.MusaitlikBaslangic;
            var end = salon.CalismaBitis < egitmen.MusaitlikBitis ? salon.CalismaBitis : egitmen.MusaitlikBitis;

            if (start.AddMinutes(sure) > end) return new List<string>();

            // tek seferde randevularý çek
            var existingSalon = await _context.Randevular
                .AsNoTracking()
                .Where(r => r.SalonId == salonId && r.Tarih == tarih && r.Durum != "Ýptal")
                .Select(r => new { r.Baslangic, r.Bitis, r.KisiSayisi })
                .ToListAsync();

            var existingEgitmen = await _context.Randevular
                .AsNoTracking()
                .Where(r => r.EgitmenId == egitmenId && r.Tarih == tarih && r.Durum != "Ýptal")
                .Select(r => new { r.Baslangic, r.Bitis })
                .ToListAsync();

            var slots = new List<string>();

            // 30 dk step
            for (var t = start; t.AddMinutes(sure) <= end; t = t.AddMinutes(30))
            {
                var tEnd = t.AddMinutes(sure);

                // eðitmen ayný anda 1 randevu
                if (existingEgitmen.Any(x => Overlaps(t, tEnd, x.Baslangic, x.Bitis)))
                    continue;

                // salon kontenjan
                var dolu = existingSalon
                    .Where(x => Overlaps(t, tEnd, x.Baslangic, x.Bitis))
                    .Sum(x => x.KisiSayisi);

                if (dolu + kisiSayisi > salon.Kontenjan)
                    continue;

                slots.Add(t.ToString("HH:mm"));
            }

            return slots;
        }
    }
}
