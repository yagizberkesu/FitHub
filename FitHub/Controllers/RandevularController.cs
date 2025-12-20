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

        // --- YARDIMCI ÖZELLİKLER (Helpers) ---

        // Session'dan kullanıcı ID'sini okuma
        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

        // Oturumdaki kullanıcının Admin olup olmadığını kontrol eder
        private bool IsAdmin
        {
            get
            {
                var role = HttpContext.Session.GetString("UserRole");
                return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            }
        }

        // --- 1. LİSTELEME (INDEX) ---
        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == null) return RedirectToAction("Login", "Uye");

            var query = _context.Randevular
                .Include(r => r.Salon)
                .Include(r => r.Egitmen)
                .Include(r => r.Uye)
                .Include(r => r.HizmetRef)
                .OrderByDescending(r => r.Tarih)
                .AsQueryable();

            // Admin değilse sadece kendi randevularını görsün
            if (!IsAdmin)
            {
                query = query.Where(r => r.UyeId == CurrentUserId.Value);
            }

            return View(await query.ToListAsync());
        }

        // --- 2. DETAYLAR (DETAILS) ---
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.Salon)
                .Include(r => r.Egitmen)
                .Include(r => r.Uye)
                .Include(r => r.HizmetRef)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null) return NotFound();

            // Güvenlik kontrolü
            if (!IsAdmin && randevu.UyeId != CurrentUserId)
            {
                return RedirectToAction("Index");
            }

            return View(randevu);
        }

        // --- 3. OLUŞTURMA (CREATE - GET) ---
        public async Task<IActionResult> Create()
        {
            if (CurrentUserId == null) return RedirectToAction("Login", "Uye");

            var model = new RandevuCreateViewModel
            {
                Salonlar = await _context.Salonlar.AsNoTracking()
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Ad })
                    .ToListAsync(),

                Egitmenler = new List<SelectListItem>(),
                Hizmetler = new List<SelectListItem>(),
                SaatSecenekleri = new List<string>()
            };

            return View(model);
        }

        // --- 4. OLUŞTURMA (CREATE - POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RandevuCreateViewModel model)
        {
            if (CurrentUserId == null) return RedirectToAction("Login", "Uye");

            if (!ModelState.IsValid)
            {
                await FillLists(model);
                return View(model);
            }

            if (!TimeOnly.TryParse(model.BaslangicStr, out TimeOnly baslangicSaat))
            {
                ModelState.AddModelError("", "Geçersiz saat formatı.");
                await FillLists(model);
                return View(model);
            }

            var hizmet = await _context.Hizmetler.FindAsync(model.HizmetId);
            if (hizmet == null)
            {
                ModelState.AddModelError("", "Hizmet bulunamadı.");
                await FillLists(model);
                return View(model);
            }

            var randevu = new Randevu
            {
                UyeId = CurrentUserId.Value,
                SalonId = model.SalonId ?? 0,
                EgitmenId = model.EgitmenId ?? 0,
                HizmetId = hizmet.Id,
                Hizmet = hizmet.Ad,
                Tarih = DateOnly.FromDateTime(model.Tarih),
                Baslangic = baslangicSaat,
                Bitis = baslangicSaat.AddMinutes(hizmet.SureDakika),
                Sure = hizmet.SureDakika,
                Ucret = hizmet.Ucret,
                KisiSayisi = model.KisiSayisi,
                Durum = "Beklemede" // Varsayılan durum
            };

            _context.Randevular.Add(randevu);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // --- 5. SİLME (DELETE - GET) ---
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            if (!IsAdmin && randevu.UyeId != CurrentUserId)
            {
                return RedirectToAction("Index");
            }

            return View(randevu);
        }

        // --- 6. SİLME / REDDETME / İPTAL (DELETE - POST) ---
        // Bu metod hem üyelerin iptali hem adminin reddetmesi için kullanılır.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                // Yetki kontrolü
                if (!IsAdmin && randevu.UyeId != CurrentUserId)
                {
                    return RedirectToAction("Index");
                }

                if (!IsAdmin)
                {
                    // Üye ise sadece iptal edebilir
                    randevu.Durum = "İptal";
                    randevu.Onaylandi = false;
                }
                else
                {
                    // Admin ise "Sil" işlemi aslında REDDETMEK demektir
                    randevu.Durum = "Reddedildi";
                    randevu.Onaylandi = false;
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // --- 7. ONAYLAMA (Sadece Admin) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int id)
        {
            if (!IsAdmin) return RedirectToAction(nameof(Index));

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            randevu.Durum = "Onaylandı";
            randevu.Onaylandi = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // --- 8. DÜZENLEME (EDIT - GET) ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            if (!IsAdmin && randevu.UyeId != CurrentUserId)
            {
                return RedirectToAction("Index");
            }

            return View(randevu);
        }

        // --- 9. DÜZENLEME (EDIT - POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Randevu randevu)
        {
            if (id != randevu.Id) return NotFound();

            var mevcutRandevu = await _context.Randevular.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (mevcutRandevu == null) return NotFound();

            if (!IsAdmin && mevcutRandevu.UyeId != CurrentUserId)
            {
                return RedirectToAction("Index");
            }

            if (randevu.Baslangic.Minute != 0 && randevu.Baslangic.Minute != 30)
            {
                ModelState.AddModelError("Baslangic", "Randevu saatleri sadece tam (00) veya buçuk (30) olabilir.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var hizmet = await _context.Hizmetler.FindAsync(mevcutRandevu.HizmetId);
                    if (hizmet != null)
                    {
                        randevu.HizmetId = mevcutRandevu.HizmetId;
                        randevu.SalonId = mevcutRandevu.SalonId;
                        randevu.EgitmenId = mevcutRandevu.EgitmenId;
                        randevu.UyeId = mevcutRandevu.UyeId;

                        randevu.Bitis = randevu.Baslangic.AddMinutes(hizmet.SureDakika);
                        randevu.Sure = hizmet.SureDakika;
                        randevu.Ucret = hizmet.Ucret;
                        randevu.Hizmet = hizmet.Ad;
                    }

                    _context.Update(randevu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Randevular.Any(e => e.Id == randevu.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(randevu);
        }

        // --- AJAX & YARDIMCI METOTLAR ---

        public async Task<JsonResult> GetEgitmenler(int salonId)
        {
            var data = await _context.Egitmenler
                .Where(e => e.SalonId == salonId)
                .Select(e => new { id = e.Id, ad = e.Ad })
                .ToListAsync();
            return Json(data);
        }

        public async Task<JsonResult> GetHizmetler(int egitmenId)
        {
            var data = await _context.EgitmenHizmetler
                .Where(eh => eh.EgitmenId == egitmenId)
                .Include(eh => eh.Hizmet)
                .Select(eh => new {
                    id = eh.Hizmet.Id,
                    ad = eh.Hizmet.Ad,
                    sure = eh.Hizmet.SureDakika,
                    ucret = eh.Hizmet.Ucret
                })
                .ToListAsync();
            return Json(data);
        }

        public async Task<JsonResult> GetSlots(int salonId, int egitmenId, DateTime tarih, int hizmetId)
        {
            var egitmen = await _context.Egitmenler.FindAsync(egitmenId);
            var hizmet = await _context.Hizmetler.FindAsync(hizmetId);

            if (egitmen == null || hizmet == null) return Json(new List<string>());

            var slots = new List<string>();
            var start = egitmen.MusaitlikBaslangic;
            var end = egitmen.MusaitlikBitis;
            var tarihDateOnly = DateOnly.FromDateTime(tarih);

            var existingRandevular = await _context.Randevular
                .Where(r => r.EgitmenId == egitmenId
                            && r.Tarih == tarihDateOnly
                            && r.Durum != "İptal"
                            && r.Durum != "Reddedildi") // Reddedilen saatler tekrar alınabilsin
                .ToListAsync();

            while (start.AddMinutes(hizmet.SureDakika) <= end)
            {
                var slotEnd = start.AddMinutes(hizmet.SureDakika);
                bool isTaken = existingRandevular.Any(r =>
                    (start >= r.Baslangic && start < r.Bitis) ||
                    (slotEnd > r.Baslangic && slotEnd <= r.Bitis) ||
                    (start <= r.Baslangic && slotEnd >= r.Bitis)
                );

                if (!isTaken) slots.Add(start.ToString("HH:mm"));
                start = start.AddMinutes(30);
            }

            return Json(slots);
        }

        private async Task FillLists(RandevuCreateViewModel model)
        {
            model.Salonlar = await _context.Salonlar.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Ad }).ToListAsync();
            if (model.SalonId.HasValue)
            {
                model.Egitmenler = await _context.Egitmenler.Where(e => e.SalonId == model.SalonId)
                    .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Ad }).ToListAsync();
            }
        }
    }
}