using System.ComponentModel.DataAnnotations;

namespace AtlasPremierProperties.Models.Entities
{
    public class Owners
    {
        public int OwnerID { get; set; }

        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email address")]
        public string EmailAddress { get; set; }

        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        public string PasswordHash { get; set; }

        // Typed by staff on the Create/Edit forms; only its hash is stored.
        [StringLength(100, MinimumLength = 10, ErrorMessage = "The portal password must be at least 10 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "Owner portal password")]
        public string PortalPassword { get; set; }

        public bool HasPortalAccess { get { return !string.IsNullOrEmpty(PasswordHash); } }

        public string FullName { get { return FirstName + " " + LastName; } }
    }
}
