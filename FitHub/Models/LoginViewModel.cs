using System.ComponentModel.DataAnnotations;

namespace FitHub.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta alaný zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Þifre alaný zorunludur.")]
        [DataType(DataType.Password)]
        public string Sifre { get; set; } = string.Empty;
    }
}
