using System;

namespace AtlasPremierProperties.Services
{
    public class SettlementResult
    {
        public decimal GrossRent { get; set; }
        public decimal MaintenanceCosts { get; set; }
        public decimal NetAmount { get; set; }
        public decimal ManagementFee { get; set; }
        public decimal OwnerPayout { get; set; }
        public int DaysOccupied { get; set; }

        public bool IsProRata
        {
            get { return DaysOccupied < 30; }
        }
    }

    public class SettlementService
    {
        private const decimal FeeRate = 0.12m;
        private const int DaysPerMonth = 30;

        public SettlementResult Calculate(
            decimal monthlyRent,
            decimal maintenanceCosts,
            int daysOccupied)
        {
            if (daysOccupied < 1 || daysOccupied > 31)
                throw new ArgumentException(
                    "Days occupied must be between 1 and 31.");

            if (maintenanceCosts < 0)
                throw new ArgumentException(
                    "Maintenance costs cannot be negative.");

            // Step 1: Pro-rata
            decimal effectiveRent = Math.Round(
                (monthlyRent / DaysPerMonth) * daysOccupied, 2);

            // Step 2: Deduct maintenance BEFORE management fee
            decimal netAmount = effectiveRent - maintenanceCosts;

            if (netAmount < 0)
                throw new InvalidOperationException(
                    "Maintenance costs exceed the rent amount.");

            // Step 3: 12% management fee on net amount
            decimal managementFee = Math.Round(
                netAmount * FeeRate, 2);

            // Step 4: Owner receives the remainder
            decimal ownerPayout = Math.Round(
                netAmount - managementFee, 2);

            return new SettlementResult
            {
                GrossRent = effectiveRent,
                MaintenanceCosts = maintenanceCosts,
                NetAmount = Math.Round(netAmount, 2),
                ManagementFee = managementFee,
                OwnerPayout = ownerPayout,
                DaysOccupied = daysOccupied
            };
        }
    }
}