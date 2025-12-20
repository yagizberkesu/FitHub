using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitHub.Models
{
    public class RandevuCreateViewModel
    {
        [Required(ErrorMessage = "Lütfen bir salon seçin.")]
        public int? SalonId { get; set; }

        [Required(ErrorMessage = "Lütfen bir eğitmen seçin.")]
        public int? EgitmenId { get; set; }

        [Required(ErrorMessage = "Lütfen bir hizmet seçin.")]
        public int? HizmetId { get; set; }

        [Required(ErrorMessage = "Tarih seçiniz.")]
        public DateTime Tarih { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Saat seçiniz.")]
        public string BaslangicStr { get; set; } = string.Empty;

        [Range(1, 50, ErrorMessage = "Kişi sayısı 1-50 arasında olmalı.")]
        public int KisiSayisi { get; set; } = 1;

        // View tarafında kullanılan listeler
        public List<SelectListItem> Salonlar { get; set; } = new();
        public List<SelectListItem> Egitmenler { get; set; } = new();
        public List<SelectListItem> Hizmetler { get; set; } = new();
        public List<string> SaatSecenekleri { get; set; } = new();
    }
}