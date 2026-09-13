using System.ComponentModel.DataAnnotations;

namespace AtlasPremierProperties.Models.ViewModels
{
    public class SetupViewModel
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Password must be at least 10 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}
