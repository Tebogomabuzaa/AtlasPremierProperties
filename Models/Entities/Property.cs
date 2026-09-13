using System.ComponentModel.DataAnnotations;

namespace AtlasPremierProperties.Models.Entities
{
    public class Property
    {
        public int PropertyID { get; set; }

        [Display(Name = "Description")]
        public string PropertyDescription { get; set; }

        [Required]
        [Display(Name = "Physical address")]
        public string PhysicalAddress { get; set; }

        [Display(Name = "Owner")]
        public int OwnerID { get; set; }

        [Display(Name = "Monthly rent (R)")]
        public decimal MonthlyRent { get; set; }

        // true means the property is vacant.
        [Display(Name = "Vacant")]
        public bool VacancyStatus { get; set; }

        // Not a database column - populated via JOIN
        public string OwnerName { get; set; }
    }
}
