using System;
using System.ComponentModel.DataAnnotations;

namespace FitHub.Models
{
    public class Randevu
    {
        public int Id { get; set; }

        [Required]
        public DateOnly Tarih { get; set; }

        // 30 dk slot baþlangýcý
        [Required]
        public TimeOnly Baslangic { get; set; }

        // Sure’den hesaplanýp doldurulur
        [Required]
        public TimeOnly Bitis { get; set; }

        // dakika (30'un katý)
        [Range(30, 240)]
        public int Sure { get; set; } = 30;

        // Salon kontenjaný için (MVP: 1 kiþi)
        [Range(1, 50)]
        public int KisiSayisi { get; set; } = 1;

        public int UyeId { get; set; }
        public Uye? Uye { get; set; }

        public int EgitmenId { get; set; }
        public Egitmen? Egitmen { get; set; }

        public int SalonId { get; set; }
        public Salon? Salon { get; set; }

        public string Hizmet { get; set; } = string.Empty;
        public decimal Ucret { get; set; }

        public bool Onaylandi { get; set; }

        // "Beklemede", "Onaylandý", "Ýptal"
        public string Durum { get; set; } = "Beklemede";
    }
}
