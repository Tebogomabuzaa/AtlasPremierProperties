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
            bool hasPassport = !string.IsNullOrWhiteSpace(tenant.PassportIDNumber);
            bool hasNationality = !string.IsNullOrWhiteSpace(tenant.Nationality);
            bool meetsIncome = tenant.DeclaredMonthlyIncome >= MinimumIncome;

            bool approved = hasPassport && hasNationality && meetsIncome;

            return new KycResult
            {
                Status = approved ? "Approved" : "Declined",
                Message = approved
                    ? "KYC verification passed. Tenant meets all requirements."
                    : "KYC verification failed. A passport or ID number, a nationality and a declared income of at least R5,000 are required."
            };
        }
    }
}
