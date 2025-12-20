using System.ComponentModel.DataAnnotations;

namespace FitHub.Models
{
    public class Hizmet
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Ad { get; set; } = string.Empty;

        // Dakika cinsinden (30'un katı olacak)
        [Range(30, 480)]
        public int SureDakika { get; set; } = 30;

        [Range(0, 100000)]
        public decimal Ucret { get; set; }

        [Required]
        public int SalonId { get; set; }
        public Salon? Salon { get; set; }

        public ICollection<EgitmenHizmet> EgitmenHizmetler { get; set; } = new List<EgitmenHizmet>();
    }
}
