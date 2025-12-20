using FitHub.Filters;
using FitHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitHub.Controllers
{
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

        // =========================================================
        // ADMIN ONAY / IPTAL
        //  - GET gelirse de çalışsın (linke tıklayınca 405 olmasın)
        //  - POST ile de çalışsın (form kullanınca doğru yol)
        // =========================================================

        [AdminAuthorize]
        [HttpGet]
        public async Task<IActionResult> Approve(int id)
        {
            var r = await _context.Randevular.FindAsync(id);
            if (r == null) return NotFound();

            r.Durum = "Onaylandı";
            r.Onaylandi = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [AdminAuthorize]
        [HttpPost, ActionName("Approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApprovePost(int id)
        {
            var r = await _context.Randevular.FindAsync(id);
            if (r == null) return NotFound();

            r.Durum = "Onaylandı";
            r.Onaylandi = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [AdminAuthorize]
        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            var r = await _context.Randevular.FindAsync(id);
            if (r == null) return NotFound();

            r.Durum = "İptal";
            r.Onaylandi = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [AdminAuthorize]
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelPost(int id)
        {
            var r = await _context.Randevular.FindAsync(id);
            if (r == null) return NotFound();

            r.Durum = "İptal";
            r.Onaylandi = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Create()
        {
            if (CurrentUserId == null) return RedirectToAction("Login", "Uye");

            var vm = new RandevuCreateViewModel();
            await FillDropDowns(vm);

            if (vm.Salonlar.Count > 0) vm.SalonId = int.Parse(vm.Salonlar[0].Value!);
            if (vm.Egitmenler.Count > 0) vm.EgitmenId = int.Parse(vm.Egitmenler[0].Value!);

            await FillHizmetler(vm);

            if (vm.Hizmetler.Count > 0) vm.HizmetId = int.Parse(vm.Hizmetler[0].Value!);

            vm.SaatSecenekleri = await ComputeAvailableSlots(vm.SalonId, vm.EgitmenId, vm.Tarih, vm.HizmetId, vm.KisiSayisi);
            if (vm.SaatSecenekleri.Count > 0) vm.BaslangicStr = vm.SaatSecenekleri[0];

            return View(vm);
        }

        // Salon/Eğitmen değişince hizmetleri getir (JSON)
        [HttpGet]
        public async Task<IActionResult> GetHizmetler(int salonId, int egitmenId)
        {
            var egitmenHizmetleri = await _context.EgitmenHizmetler
                .AsNoTracking()
                .Where(x => x.EgitmenId == egitmenId)
                .Select(x => x.Hizmet!)
                .Where(h => h.SalonId == salonId)
                .Select(h => new { h.Id, h.Ad, h.SureDakika, h.Ucret })
                .ToListAsync();

            if (egitmenHizmetleri.Count > 0)
                return Json(egitmenHizmetleri);

            var salonHizmetleri = await _context.Hizmetler
                .AsNoTracking()
                .Where(h => h.SalonId == salonId)
                .Select(h => new { h.Id, h.Ad, h.SureDakika, h.Ucret })
                .ToListAsync();

            return Json(salonHizmetleri);
        }

        // Uygun saatler
        [HttpGet]
        public async Task<IActionResult> GetSlots(int salonId, int egitmenId, string tarih, int hizmetId, int kisiSayisi)
        {
            if (CurrentUserId == null) return Unauthorized();

            if (!DateOnly.TryParse(tarih, out var date))
                return BadRequest("tarih formatı yanlış (yyyy-MM-dd)");

            if (kisiSayisi <= 0) kisiSayisi = 1;

            var slots = await ComputeAvailableSlots(salonId, egitmenId, date, hizmetId, kisiSayisi);
            return Json(slots);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RandevuCreateViewModel vm)
        {
            if (CurrentUserId == null) return RedirectToAction("Login", "Uye");

            await FillDropDowns(vm);
            await FillHizmetler(vm);

            if (!TimeOnly.TryParseExact(vm.BaslangicStr, "HH:mm", out var baslangic))
                ModelState.AddModelError(nameof(vm.BaslangicStr), "Geçersiz saat seçimi.");

            var hizmet = await _context.Hizmetler.AsNoTracking().FirstOrDefaultAsync(h => h.Id == vm.HizmetId);
            if (hizmet == null)
                ModelState.AddModelError(nameof(vm.HizmetId), "Hizmet seçiniz.");

            if (hizmet != null && hizmet.SureDakika % 30 != 0)
                ModelState.AddModelError(nameof(vm.HizmetId), "Hizmet süresi 30 dakikanın katı olmalı.");

            if (!ModelState.IsValid)
            {
                vm.SaatSecenekleri = await ComputeAvailableSlots(vm.SalonId, vm.EgitmenId, vm.Tarih, vm.HizmetId, vm.KisiSayisi);
                return View(vm);
            }

            var sure = hizmet!.SureDakika;
            var bitis = baslangic.AddMinutes(sure);

            var available = await ComputeAvailableSlots(vm.SalonId, vm.EgitmenId, vm.Tarih, vm.HizmetId, vm.KisiSayisi);
            if (!available.Contains(vm.BaslangicStr))
            {
                ModelState.AddModelError("", "Seçtiğiniz saat artık uygun değil. Lütfen tekrar seçin.");
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
                Sure = sure,
                KisiSayisi = vm.KisiSayisi,
                HizmetId = vm.HizmetId,
                Hizmet = hizmet.Ad,
                Ucret = hizmet.Ucret,
                Durum = "Beklemede",
                Onaylandi = false
            };

            _context.Randevular.Add(entity);
            await _context.SaveChangesAsync();
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

        private async Task FillHizmetler(RandevuCreateViewModel vm)
        {
            var q = _context.EgitmenHizmetler
                .AsNoTracking()
                .Where(x => x.EgitmenId == vm.EgitmenId)
                .Select(x => x.Hizmet!)
                .Where(h => h.SalonId == vm.SalonId);

            var list = await q.ToListAsync();

            if (list.Count == 0)
            {
                list = await _context.Hizmetler.AsNoTracking()
                    .Where(h => h.SalonId == vm.SalonId)
                    .ToListAsync();
            }

            vm.Hizmetler = list
                .OrderBy(h => h.Ad)
                .Select(h => new SelectListItem
                {
                    Value = h.Id.ToString(),
                    Text = $"{h.Ad} ({h.SureDakika} dk - {h.Ucret} ₺)"
                })
                .ToList();
        }

        private static bool Overlaps(TimeOnly aStart, TimeOnly aEnd, TimeOnly bStart, TimeOnly bEnd)
            => aStart < bEnd && aEnd > bStart;

        private async Task<List<string>> ComputeAvailableSlots(int salonId, int egitmenId, DateOnly tarih, int hizmetId, int kisiSayisi)
        {
            var salon = await _context.Salonlar.AsNoTracking().FirstOrDefaultAsync(s => s.Id == salonId);
            var egitmen = await _context.Egitmenler.AsNoTracking().FirstOrDefaultAsync(e => e.Id == egitmenId);
            var hizmet = await _context.Hizmetler.AsNoTracking().FirstOrDefaultAsync(h => h.Id == hizmetId);

            if (salon == null || egitmen == null || hizmet == null) return new List<string>();

            var sure = hizmet.SureDakika;
            if (sure <= 0 || sure % 30 != 0) return new List<string>();

            var start = salon.CalismaBaslangic > egitmen.MusaitlikBaslangic ? salon.CalismaBaslangic : egitmen.MusaitlikBaslangic;
            var end = salon.CalismaBitis < egitmen.MusaitlikBitis ? salon.CalismaBitis : egitmen.MusaitlikBitis;

            if (start.AddMinutes(sure) > end) return new List<string>();

            var existingSalon = await _context.Randevular
                .AsNoTracking()
                .Where(r => r.SalonId == salonId && r.Tarih == tarih && r.Durum != "İptal")
                .Select(r => new { r.Baslangic, r.Bitis, r.KisiSayisi })
                .ToListAsync();

            var existingEgitmen = await _context.Randevular
                .AsNoTracking()
                .Where(r => r.EgitmenId == egitmenId && r.Tarih == tarih && r.Durum != "İptal")
                .Select(r => new { r.Baslangic, r.Bitis })
                .ToListAsync();

            var slots = new List<string>();

            for (var t = start; t.AddMinutes(sure) <= end; t = t.AddMinutes(30))
            {
                var tEnd = t.AddMinutes(sure);

                if (existingEgitmen.Any(x => Overlaps(t, tEnd, x.Baslangic, x.Bitis)))
                    continue;

                var dolu = existingSalon.Where(x => Overlaps(t, tEnd, x.Baslangic, x.Bitis)).Sum(x => x.KisiSayisi);
                if (dolu + kisiSayisi > salon.Kontenjan)
                    continue;

                slots.Add(t.ToString("HH:mm"));
            }

            return slots;
        }
    }
}
