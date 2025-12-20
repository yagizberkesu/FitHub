using System.ComponentModel.DataAnnotations;

namespace FitHub.Models
{
    public class AiCoachViewModel
    {
        [Range(10, 90)]
        public int Yas { get; set; } = 22;

        [Range(120, 230)]
        public int BoyCm { get; set; } = 175;

        [Range(30, 250)]
        public int KiloKg { get; set; } = 75;

        [Required]
        public string Hedef { get; set; } = "Kas kazanımı";

        [Required]
        public string Seviye { get; set; } = "Başlangıç";

        [Range(1, 7)]
        public int HaftadaGun { get; set; } = 3;

        [Range(20, 180)]
        public int Dakika { get; set; } = 60;

        [Required]
        public string Ekipman { get; set; } = "Salon";

        [StringLength(300)]
        public string? Notlar { get; set; }
    }
}
