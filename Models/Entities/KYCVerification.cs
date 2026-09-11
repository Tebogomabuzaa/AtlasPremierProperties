using System;

namespace AtlasPremierProperties.Models.Entities
{
    public class KYCVerification
    {
        public int KYCID { get; set; }
        public int TenantID { get; set; }  // FK
        public string DocumentType { get; set; }
        public string VerificationStatus { get; set; }
        public DateTime VerifiedDate { get; set; }

        // Optional display field
        public string TenantName { get; set; }
    }
}