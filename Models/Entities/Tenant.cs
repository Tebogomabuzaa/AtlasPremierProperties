using System.ComponentModel.DataAnnotations;

namespace AtlasPremierProperties.Models.Entities
{
    // Mirrors one row in the Tenants table
    public class Tenant
    {
        public int TenantID { get; set; }

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

        [Display(Name = "Passport / ID number")]
        public string PassportIDNumber { get; set; }

        public string Nationality { get; set; }

        [Display(Name = "Declared monthly income (R)")]
        public decimal DeclaredMonthlyIncome { get; set; }

        [Display(Name = "Verification status")]
        public string VerificationStatus { get; set; }

        // Convenience property for display - not a database column
        public string FullName { get { return FirstName + " " + LastName; } }
    }
}
