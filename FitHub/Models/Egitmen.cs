using System;
using System.ComponentModel.DataAnnotations;

namespace FitHub.Models
{
    public class Egitmen
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Ad { get; set; } = string.Empty;

        public string UzmanlikAlanlari { get; set; } = string.Empty;
        public string Hizmetler { get; set; } = string.Empty;

        // Seviye 1: Tek aralýk müsaitlik saati
        public TimeOnly MusaitlikBaslangic { get; set; } = new TimeOnly(9, 0);
        public TimeOnly MusaitlikBitis { get; set; } = new TimeOnly(22, 0);

        [Required]
        public int SalonId { get; set; }

        public Salon? Salon { get; set; }
    }
}
