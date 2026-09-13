using System;
using System.Collections.Generic;
using System.Data.OleDb;
using AtlasPremierProperties.Helpers;
using AtlasPremierProperties.Models.Entities;

namespace AtlasPremierProperties.Repositories
{
    public class SettlementRepository
    {
        private readonly DatabaseHelper _db;

        public SettlementRepository()
        {
            _db = new DatabaseHelper();
        }

        public List<Settlement> GetAll()
        {
            var settlements = new List<Settlement>();

            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string sql = "SELECT SettlementID, LeaseID, GrossRent, " +
                             "MaintenanceCosts, NetAmount, ManagementFee, " +
                             "OwnerPayout, DaysOccupied, SettlementDate, " +
                             "CryptoInvoiceLink " +
                             "FROM Settlements";

                using (var cmd = new OleDbCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        settlements.Add(new Settlement
                        {
                            SettlementID = Convert.ToInt32(reader["SettlementID"]),
                            LeaseID = Convert.ToInt32(reader["LeaseID"]),
                            GrossRent = Convert.ToDecimal(reader["GrossRent"]),
                            MaintenanceCosts = Convert.ToDecimal(reader["MaintenanceCosts"]),
                            NetAmount = Convert.ToDecimal(reader["NetAmount"]),
                            ManagementFee = Convert.ToDecimal(reader["ManagementFee"]),
                            OwnerPayout = Convert.ToDecimal(reader["OwnerPayout"]),
                            DaysOccupied = Convert.ToInt32(reader["DaysOccupied"]),
                            SettlementDate = Convert.ToDateTime(reader["SettlementDate"]),
                            CryptoInvoiceLink = reader["CryptoInvoiceLink"].ToString()
                        });
                    }
                }
            }

            return settlements;
        }

        public int Add(Settlement settlement)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string sql = "INSERT INTO Settlements " +
                             "(LeaseID, GrossRent, MaintenanceCosts, NetAmount, " +
                             "ManagementFee, OwnerPayout, DaysOccupied, " +
                             "SettlementDate, CryptoInvoiceLink) " +
                             "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    // Access rejects untyped decimals with cents and DateTimes with milliseconds ("Data type mismatch"),
                    // so money goes in as Currency and the timestamp as Date.
                    cmd.Parameters.AddWithValue("@leaseId", settlement.LeaseID);
                    cmd.Parameters.Add("@grossRent", OleDbType.Currency).Value = settlement.GrossRent;
                    cmd.Parameters.Add("@maintenance", OleDbType.Currency).Value = settlement.MaintenanceCosts;
                    cmd.Parameters.Add("@netAmount", OleDbType.Currency).Value = settlement.NetAmount;
                    cmd.Parameters.Add("@managementFee", OleDbType.Currency).Value = settlement.ManagementFee;
                    cmd.Parameters.Add("@ownerPayout", OleDbType.Currency).Value = settlement.OwnerPayout;
                    cmd.Parameters.AddWithValue("@daysOccupied", settlement.DaysOccupied);
                    cmd.Parameters.Add("@settlementDate", OleDbType.Date).Value = settlement.SettlementDate;
                    cmd.Parameters.AddWithValue("@invoiceLink", settlement.CryptoInvoiceLink);

                    cmd.ExecuteNonQuery();
                }

                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                {
                    return Convert.ToInt32(idCmd.ExecuteScalar());
                }
            }
        }
    }
}