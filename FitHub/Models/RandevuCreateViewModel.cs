using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitHub.Models
{
    public class RandevuCreateViewModel
    {
        [Required]
        public int SalonId { get; set; }

        [Required]
        public int EgitmenId { get; set; }

        [Required]
        public int HizmetId { get; set; }

        [Required]
        public DateOnly Tarih { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Range(1, 50)]
        public int KisiSayisi { get; set; } = 1;

        // dropdown: "09:00", "09:30" ...
        [Required]
        public string BaslangicStr { get; set; } = "09:00";

        public List<SelectListItem> Salonlar { get; set; } = new();
        public List<SelectListItem> Egitmenler { get; set; } = new();
        public List<SelectListItem> Hizmetler { get; set; } = new();

        public List<string> SaatSecenekleri { get; set; } = new();
    }
}
