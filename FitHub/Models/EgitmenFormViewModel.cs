using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitHub.Models
{
    public class EgitmenFormViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Ad { get; set; } = string.Empty;

        public string UzmanlikAlanlari { get; set; } = string.Empty;

        public TimeOnly MusaitlikBaslangic { get; set; } = new TimeOnly(9, 0);
        public TimeOnly MusaitlikBitis { get; set; } = new TimeOnly(22, 0);

        [Required]
        public int SalonId { get; set; }

        // Eğitmenin vereceği hizmetler (çoklu seçim)
        public List<int> SelectedHizmetIds { get; set; } = new();

        public List<SelectListItem> Salonlar { get; set; } = new();
        public List<SelectListItem> Hizmetler { get; set; } = new();
    }
}
