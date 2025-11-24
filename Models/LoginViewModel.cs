using System.ComponentModel.DataAnnotations;

namespace FitHub.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Þifre zorunludur.")]
        [DataType(DataType.Password)]
        public string Sifre { get; set; }
    }
}
