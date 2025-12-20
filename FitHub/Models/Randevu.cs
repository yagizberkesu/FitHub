using System.ComponentModel.DataAnnotations;

namespace FitHub.Models
{
    public class Randevu
    {
        public int Id { get; set; }

        [Required]
        public DateOnly Tarih { get; set; }

        [Required]
        public TimeOnly Baslangic { get; set; }

        [Required]
        public TimeOnly Bitis { get; set; }

        // Hizmetten gelecek (30'un katý)
        [Range(30, 480)]
        public int Sure { get; set; } = 30;

        [Range(1, 50)]
        public int KisiSayisi { get; set; } = 1;

        public int UyeId { get; set; }
        public Uye? Uye { get; set; }

        public int EgitmenId { get; set; }
        public Egitmen? Egitmen { get; set; }

        public int SalonId { get; set; }
        public Salon? Salon { get; set; }

        // Yeni: Hizmet FK
        [Required]
        public int HizmetId { get; set; }
        public Hizmet? HizmetRef { get; set; }

        // Snapshot alanlar (listeleme için pratik)
        public string Hizmet { get; set; } = string.Empty;

        public decimal Ucret { get; set; }

        public bool Onaylandi { get; set; }
        public string Durum { get; set; } = "Beklemede";
    }
}
