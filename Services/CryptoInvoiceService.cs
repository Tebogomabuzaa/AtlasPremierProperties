using System;

namespace AtlasPremierProperties.Services
{
    public class CryptoInvoiceService
    {
        // Generates a unique payment link using a GUID.
        // In production this would call a real payment gateway API.
        public string GenerateInvoiceLink(int leaseId, decimal amount)
        {
            string uniqueId = Guid.NewGuid().ToString();

            return string.Format(
                "https://pay.cryptonics.app/invoice/{0}?lease={1}&amount={2:F2}",
                uniqueId,
                leaseId,
                amount
            );
        }
    }
}