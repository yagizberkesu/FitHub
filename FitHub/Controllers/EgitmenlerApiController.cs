using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitHub.Models;

namespace FitHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EgitmenlerApiController : ControllerBase
    {
        private readonly FitHubContext _context;

        public EgitmenlerApiController(FitHubContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var egitmenler = await _context.Egitmenler
                .Select(e => new
                {
                    e.Id,
                    e.Ad,
                    e.UzmanlikAlanlari,
                    e.Hizmetler,
                    Musaitlik = e.MusaitlikBaslangic.ToString("HH:mm") + "-" + e.MusaitlikBitis.ToString("HH:mm"),
                    e.SalonId
                })
                .ToListAsync();

            return Ok(egitmenler);
        }

        [HttpGet("uzmanlik")]
        public async Task<ActionResult> GetByUzmanlik([FromQuery] string alan)
        {
            if (string.IsNullOrWhiteSpace(alan))
                return BadRequest("Uzmanlýk alaný belirtilmelidir.");

            var needle = alan.ToLower();

            var egitmenler = await _context.Egitmenler
                .Where(e => e.UzmanlikAlanlari != null &&
                            EF.Functions.Like(e.UzmanlikAlanlari.ToLower(), $"%{needle}%"))
                .Select(e => new
                {
                    e.Id,
                    e.Ad,
                    e.UzmanlikAlanlari,
                    e.Hizmetler,
                    Musaitlik = e.MusaitlikBaslangic.ToString("HH:mm") + "-" + e.MusaitlikBitis.ToString("HH:mm"),
                    e.SalonId
                })
                .ToListAsync();

            return Ok(egitmenler);
        }

        [HttpGet("/api/uygun-egitmenler")]
        public async Task<ActionResult> GetAvailableEgitmenler([FromQuery] DateTime tarih, [FromQuery] int? salonId)
        {
            var date = DateOnly.FromDateTime(tarih);
            var time = TimeOnly.FromDateTime(tarih);

            var egitmenlerQuery = _context.Egitmenler.AsQueryable();
            if (salonId.HasValue)
                egitmenlerQuery = egitmenlerQuery.Where(e => e.SalonId == salonId.Value);

            var busyEgitmenIds = await _context.Randevular
                .Where(r => r.Tarih == date && r.Durum != "Ýptal" && r.Baslangic <= time && r.Bitis > time)
                .Select(r => r.EgitmenId)
                .Distinct()
                .ToListAsync();

            var available = await egitmenlerQuery
                .Where(e => !busyEgitmenIds.Contains(e.Id))
                .Select(e => new
                {
                    e.Id,
                    e.Ad,
                    e.UzmanlikAlanlari,
                    e.Hizmetler,
                    Musaitlik = e.MusaitlikBaslangic.ToString("HH:mm") + "-" + e.MusaitlikBitis.ToString("HH:mm"),
                    e.SalonId
                })
                .ToListAsync();

            return Ok(available);
        }
    }
}
