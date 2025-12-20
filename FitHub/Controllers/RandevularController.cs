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

        // DÜZELTME: Login olurken SetInt32 yaptığın için burada GetInt32 ile okumalıyız.
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
            // Giriş yapmamışsa Login'e at
            if (CurrentUserId == null) return RedirectToAction("Login", "Uye");

            var query = _context.Randevular
                .Include(r => r.Salon)
                .Include(r => r.Egitmen)
                .Include(r => r.Uye)
                .Include(r => r.HizmetRef)
                .OrderByDescending(r => r.Tarih)
                .AsQueryable();

            // KİLİT NOKTA: Eğer Admin DEĞİLSE, sadece kendi randevularını filtrele
            // "Başkasının randevusunu görmesin" kuralı burada çalışır.
            if (!IsAdmin)
            {
                // Veritabanındaki UyeId, oturumdaki ID'ye eşit olanları getir
                query = query.Where(r => r.UyeId == CurrentUserId.Value);
            }

            // Admin ise 'Where' çalışmaz, tüm liste gelir.
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

            // Güvenlik: Admin değilse VE randevu kendisine ait değilse göremez
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
                Durum = "Beklemede"
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

            // Güvenlik: Başkasının randevusunu silemesin
            if (!IsAdmin && randevu.UyeId != CurrentUserId)
            {
                return RedirectToAction("Index");
            }

            return View(randevu);
        }

        // --- 6. SİLME ONAY (DELETE - POST) ---
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                if (!IsAdmin && randevu.UyeId != CurrentUserId)
                {
                    return RedirectToAction("Index");
                }

                // Admin silebilir, Üye iptal edebilir
                if (!IsAdmin)
                {
                    randevu.Durum = "İptal";
                    randevu.Onaylandi = false;
                }
                else
                {
                    _context.Randevular.Remove(randevu);
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // --- RANDEVU DÜZENLEME (GET) ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            // Güvenlik: Sadece Admin veya Randevu Sahibi düzenleyebilir
            if (!IsAdmin && randevu.UyeId != CurrentUserId)
            {
                return RedirectToAction("Index");
            }

            return View(randevu);
        }

        // --- RANDEVU DÜZENLEME (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Randevu randevu)
        {
            if (id != randevu.Id) return NotFound();

            // Güvenlik Kontrolü
            var mevcutRandevu = await _context.Randevular.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (mevcutRandevu == null) return NotFound();

            if (!IsAdmin && mevcutRandevu.UyeId != CurrentUserId)
            {
                return RedirectToAction("Index");
            }

            // Dakika Kontrolü: 30 dakika kuralı (Backend Validasyonu)
            if (randevu.Baslangic.Minute != 0 && randevu.Baslangic.Minute != 30)
            {
                ModelState.AddModelError("Baslangic", "Randevu saatleri sadece tam (00) veya buçuk (30) olabilir.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Hizmet süresine göre Bitiş saatini otomatik güncelle
                    // (Hizmet süresini veritabanından tekrar çekiyoruz ki tutarlı olsun)
                    var hizmet = await _context.Hizmetler.FindAsync(mevcutRandevu.HizmetId);
                    if (hizmet != null)
                    {
                        randevu.HizmetId = mevcutRandevu.HizmetId; // Değişmemeli
                        randevu.SalonId = mevcutRandevu.SalonId;   // Değişmemeli
                        randevu.EgitmenId = mevcutRandevu.EgitmenId; // Değişmemeli
                        randevu.UyeId = mevcutRandevu.UyeId; // Değişmemeli

                        // Bitiş saatini hesapla
                        randevu.Bitis = randevu.Baslangic.AddMinutes(hizmet.SureDakika);
                        randevu.Sure = hizmet.SureDakika;
                        randevu.Ucret = hizmet.Ucret;

                        // Diğer alanları koru
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
                            && r.Durum != "İptal")
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