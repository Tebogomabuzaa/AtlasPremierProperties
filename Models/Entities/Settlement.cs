using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AtlasPremierProperties.Models.Entities
{
    // Mirrors one row in the Settlements table
    public class Settlement
    {
        public int SettlementID { get; set; }
        public int LeaseID { get; set; }
        public decimal GrossRent { get; set; }
        public decimal MaintenanceCosts { get; set; }
        public decimal NetAmount { get; set; }
        public decimal ManagementFee { get; set; }
        public decimal OwnerPayout { get; set; }
        public int DaysOccupied { get; set; }
        public DateTime SettlementDate { get; set; }
        public string CryptoInvoiceLink { get; set; }
    }
}