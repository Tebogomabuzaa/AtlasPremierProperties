using System;
using System.Collections.Generic;
using System.Globalization;

namespace AtlasPremierProperties.Services
{
    public class CryptoInvoiceService
    {
        // Currency code and display name for each coin an owner payout can be settled in.
        public static readonly IDictionary<string, string> Currencies = new Dictionary<string, string>
        {
            { "BTC", "Bitcoin (BTC)" },
            { "ETH", "Ethereum (ETH)" },
            { "USDT", "Tether (USDT)" },
            { "USDC", "USD Coin (USDC)" }
        };

        public string GenerateInvoiceLink(int leaseId, decimal amount)
        {
            return GenerateInvoiceLink(leaseId, amount, "BTC");
        }

        // Generates a unique payment link using a GUID. The amount is in rand; currency is the coin to pay in.
        // In production this would call a real payment gateway API.
        public string GenerateInvoiceLink(int leaseId, decimal amount, string currency)
        {
            if (currency == null || !Currencies.ContainsKey(currency))
                throw new ArgumentException("Choose a supported settlement currency.");

            string uniqueId = Guid.NewGuid().ToString();

            return string.Format(CultureInfo.InvariantCulture,
                "https://pay.cryptonics.app/invoice/{0}?lease={1}&amount={2:F2}&fiat=ZAR&currency={3}",
                uniqueId, leaseId, amount, currency);
        }
    }
}
