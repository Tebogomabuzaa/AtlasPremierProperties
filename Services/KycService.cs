using AtlasPremierProperties.Models.Entities;

namespace AtlasPremierProperties.Services
{
    public class KycResult
    {
        public string Status { get; set; } // "Approved" or "Declined"
        public string Message { get; set; }
    }

    public class KycService
    {
        private const decimal MinimumIncome = 5000m;

        // Simulates a KYC API call using internal business rules.
        // In production this would call an external verification service.
        public KycResult VerifyTenant(Tenant tenant)
        {
            bool hasPassport = !string.IsNullOrEmpty(tenant.PassportIDNumber);
            bool hasNationality = !string.IsNullOrEmpty(tenant.Nationality);
            bool meetsIncome = tenant.DeclaredMonthlyIncome >= MinimumIncome;

            bool approved = hasPassport && hasNationality && meetsIncome;

            return new KycResult
            {
                Status = approved ? "Approved" : "Declined",
                Message = approved ? "KYC verification passed. Tenant meets all requirements." : "KYC verification failed. Requirements not met."
            };
        }
    }
}
