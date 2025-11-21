using System;
//a
namespace FitHub.Models
{
    public class Randevu
    {
        public int Id { get; set; }
        public DateTime Tarih { get; set; }
        public string Saat { get; set; }
        public int UyeId { get; set; }
        public Uye Uye { get; set; }
        public int EgitmenId { get; set; }
        public Egitmen Egitmen { get; set; }
        public int SalonId { get; set; }
        public Salon Salon { get; set; }
        public string Hizmet { get; set; }
        public int Sure { get; set; } // dakika cinsinden
        public decimal Ucret { get; set; }
        public bool Onaylandi { get; set; }
        
    }
}
