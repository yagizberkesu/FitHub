using System;
using System.ComponentModel.DataAnnotations;

namespace FitHub.Models
{
    public class Salon
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Ad { get; set; } = string.Empty;

        // Seviye 1: Tek aralýk çalýþma saati
        [Required]
        public TimeOnly CalismaBaslangic { get; set; } = new TimeOnly(9, 0);

        [Required]
        public TimeOnly CalismaBitis { get; set; } = new TimeOnly(22, 0);

        // Ayný saat aralýðýnda kaç kiþi bulunabilir?
        [Range(1, 500)]
        public int Kontenjan { get; set; } = 10;

        // Þimdilik string olarak kalsýn
        public string Hizmetler { get; set; } = string.Empty;
    }
}
