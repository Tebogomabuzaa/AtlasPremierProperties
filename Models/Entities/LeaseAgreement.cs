using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AtlasPremierProperties.Models.Entities
{
    // Mirrors one row in the LeaseAgreements table
    public class LeaseAgreement
    {
        public int LeaseID { get; set; }
        public int PropertyID { get; set; }
        public int TenantID { get; set; }
        public int ManagerID { get; set; }
        public DateTime LeaseStartDate { get; set; }
        public DateTime LeaseEndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public string LeaseStatus { get; set; }

        // Populated via JSONs, not stored in this table
        public string PropertyAddress { get; set; }
        public string TenantName { get; set; }
        public string ManagerName { get; set; }
    }
}