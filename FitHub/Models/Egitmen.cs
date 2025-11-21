namespace FitHub.Models
{
    public class Egitmen
    {
        public int Id { get; set; }
        public string Ad { get; set; }
        public string UzmanlikAlanlari { get; set; } // Örn: "Kas Geliþtirme, Yoga"
        public string Hizmetler { get; set; } // Verdiði hizmet türleri
        public string MusaitlikSaatleri { get; set; } // Örn: "09:00-12:00, 14:00-18:00"
        public int SalonId { get; set; } // Hangi salonda çalýþýyor
        public Salon Salon { get; set; }
    }
}
