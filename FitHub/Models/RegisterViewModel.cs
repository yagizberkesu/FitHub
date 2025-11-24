using System.ComponentModel.DataAnnotations;

namespace FitHub.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad gereklidir.")]
        [StringLength(50)]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Soyad gereklidir.")]
        [StringLength(50)]
        public string Soyad { get; set; }

        [Required(ErrorMessage = "E-posta gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Þifre gereklidir.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Þifre en az 6 karakter olmalýdýr.")]
        [DataType(DataType.Password)]
        public string Sifre { get; set; }
    }
}
