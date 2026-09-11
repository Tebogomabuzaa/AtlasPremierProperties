using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AtlasPremierProperties.Models.Entities
{
    // Mirrors one row in the Tenants table
    public class Tenant
    {
        public int TenantID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string PassportIDNumber { get; set; }
        public string Nationality { get; set; }
        public decimal DeclaredMonthlyIncome { get; set; }
        public string VerificationStatus { get; set; }

        // Convenience property for display - not a database column
        public string FullName { get { return FirstName + " " + LastName; } }
    }
}