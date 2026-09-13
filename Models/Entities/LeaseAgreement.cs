using System;
using System.ComponentModel.DataAnnotations;

namespace AtlasPremierProperties.Models.Entities
{
    // Mirrors one row in the LeaseAgreements table
    public class LeaseAgreement
    {
        // Pending and Active leases hold a property; Ended and Terminated leases free it up.
        public static readonly string[] Statuses = { "Pending", "Active", "Ended", "Terminated" };

        public int LeaseID { get; set; }

        [Required(ErrorMessage = "Select a property.")]
        [Display(Name = "Property")]
        public int PropertyID { get; set; }

        [Required(ErrorMessage = "Select a tenant.")]
        [Display(Name = "Tenant")]
        public int TenantID { get; set; }

        [Required(ErrorMessage = "Select a property manager.")]
        [Display(Name = "Property manager")]
        public int ManagerID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime LeaseStartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime LeaseEndDate { get; set; }

        [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Monthly rent must be more than zero.")]
        [Display(Name = "Monthly rent (R)")]
        public decimal MonthlyRent { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string LeaseStatus { get; set; }

        // Populated via JOINs, not stored in this table
        public string PropertyAddress { get; set; }
        public string TenantName { get; set; }
        public string ManagerName { get; set; }

        public bool IsOpen
        {
            get { return LeaseStatus == "Pending" || LeaseStatus == "Active"; }
        }
    }
}
