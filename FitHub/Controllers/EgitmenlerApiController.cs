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

        // GET: api/egitmenler
        // Returns all trainers with selected fields
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
                    e.MusaitlikSaatleri,
                    e.SalonId
                })
                .ToListAsync();

            return Ok(egitmenler);
        }

        // GET: api/egitmenler/uzmanlik?alan=Yoga
        // Returns trainers filtered by specialty (case-insensitive)
        [HttpGet("uzmanlik")]
        public async Task<ActionResult> GetByUzmanlik([FromQuery] string alan)
        {
            if (string.IsNullOrWhiteSpace(alan))
                return BadRequest("Uzmanlýk alaný belirtilmelidir.");

            var egitmenler = await _context.Egitmenler
                .Where(e => e.UzmanlikAlanlari != null &&
                            EF.Functions.Like(e.UzmanlikAlanlari.ToLower(), $"%{alan.ToLower()}%"))
                .Select(e => new
                {
                    e.Id,
                    e.Ad,
                    e.UzmanlikAlanlari,
                    e.Hizmetler,
                    e.MusaitlikSaatleri,
                    e.SalonId
                })
                .ToListAsync();

            return Ok(egitmenler);
        }

        // GET: api/uygun-egitmenler?tarih=2025-12-01T10:00:00&salonId=1
        // Returns trainers who do NOT have an appointment at the given date/time
        [HttpGet("/api/uygun-egitmenler")]
        public async Task<ActionResult> GetAvailableEgitmenler([FromQuery] DateTime tarih, [FromQuery] int? salonId)
        {
            // Find all trainers
            var egitmenlerQuery = _context.Egitmenler.AsQueryable();

            if (salonId.HasValue)
                egitmenlerQuery = egitmenlerQuery.Where(e => e.SalonId == salonId.Value);

            var egitmenler = await egitmenlerQuery.ToListAsync();

            // Find trainer IDs with a non-cancelled appointment at the given date
            var busyEgitmenIds = await _context.Randevular
                .Where(r => r.Tarih.Date == tarih.Date && r.Durum != "Ýptal")
                .Select(r => r.EgitmenId)
                .Distinct()
                .ToListAsync();

            // Filter out busy trainers
            var available = egitmenler
                .Where(e => !busyEgitmenIds.Contains(e.Id))
                .Select(e => new
                {
                    e.Id,
                    e.Ad,
                    e.UzmanlikAlanlari,
                    e.Hizmetler,
                    e.MusaitlikSaatleri,
                    e.SalonId
                })
                .ToList();

            return Ok(available);
        }
    }
}
