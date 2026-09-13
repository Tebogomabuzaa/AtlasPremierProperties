using System.ComponentModel.DataAnnotations;

namespace AtlasPremierProperties.Models.ViewModels
{
    public class OwnerLoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
