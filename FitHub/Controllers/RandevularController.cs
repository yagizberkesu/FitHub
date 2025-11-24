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
            // Çakýþma kontrolü
            var baslangic = randevu.Tarih.Date.Add(TimeSpan.Parse(randevu.Saat));
            var bitis = baslangic.AddMinutes(randevu.Sure);

            var conflicts = await _context.Randevular
                .Where(r =>
                    r.EgitmenId == randevu.EgitmenId &&
                    r.Durum != "Ýptal" &&
                    r.Tarih.Date == randevu.Tarih.Date &&
                    (
                        // Zaman aralýðý çak
