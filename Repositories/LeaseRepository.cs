using System;
using System.Collections.Generic;
using System.Data.OleDb;
using AtlasPremierProperties.Helpers;
using AtlasPremierProperties.Models.Entities;

namespace AtlasPremierProperties.Repositories
{
    public class LeaseRepository
    {
        // Access needs each extra JOIN wrapped in parentheses.
        private const string SelectSql =
            "SELECT la.*, " +
            "p.PhysicalAddress AS PropertyAddress, " +
            "t.FirstName & ' ' & t.LastName AS TenantName, " +
            "pm.FirstName & ' ' & pm.LastName AS ManagerName " +
            "FROM ((LeaseAgreements la " +
            "LEFT JOIN Properties p ON la.PropertyID = p.PropertyID) " +
            "LEFT JOIN Tenants t ON la.TenantID = t.TenantID) " +
            "LEFT JOIN PropertyManagers pm ON la.ManagerID = pm.ManagerID";

        private readonly DatabaseHelper _db;

        public LeaseRepository()
        {
            _db = new DatabaseHelper();
        }

        public List<LeaseAgreement> GetAll()
        {
            var list = new List<LeaseAgreement>();

            using (var conn = _db.GetConnection())
            using (var cmd = new OleDbCommand(SelectSql + " ORDER BY la.LeaseStartDate DESC, la.LeaseID DESC", conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(Map(reader));
                    }
                }
            }

            return list;
        }

        public LeaseAgreement GetById(int id)
        {
            using (var conn = _db.GetConnection())
            using (var cmd = new OleDbCommand(SelectSql + " WHERE la.LeaseID = ?", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) return Map(reader);
                }
            }

            return null;
        }

        public int Add(LeaseAgreement lease)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                var sql =
                    "INSERT INTO LeaseAgreements " +
                    "(PropertyID, TenantID, ManagerID, LeaseStartDate, " +
                    "LeaseEndDate, MonthlyRent, LeaseStatus) " +
                    "VALUES (?, ?, ?, ?, ?, ?, ?)";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    AddFieldParameters(cmd, lease);
                    cmd.ExecuteNonQuery();
                }

                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                {
                    return Convert.ToInt32(idCmd.ExecuteScalar());
                }
            }
        }

        public void Update(LeaseAgreement lease)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                var sql =
                    "UPDATE LeaseAgreements SET " +
                    "PropertyID = ?, " +
                    "TenantID = ?, " +
                    "ManagerID = ?, " +
                    "LeaseStartDate = ?, " +
                    "LeaseEndDate = ?, " +
                    "MonthlyRent = ?, " +
                    "LeaseStatus = ? " +
                    "WHERE LeaseID = ?";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    AddFieldParameters(cmd, lease);
                    cmd.Parameters.AddWithValue("@id", lease.LeaseID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool HasSettlements(int leaseId)
        {
            using (var conn = _db.GetConnection())
            using (var cmd = new OleDbCommand("SELECT COUNT(*) FROM Settlements WHERE LeaseID = ?", conn))
            {
                cmd.Parameters.AddWithValue("@id", leaseId);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public void Delete(int leaseId)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                using (var cmd = new OleDbCommand(
                    "DELETE FROM LeaseAgreements WHERE LeaseID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", leaseId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Parameters are positional, so they must follow the column order used in Add and Update.
        // AddWithValue sends decimals and DateTimes with types Access rejects, so rent and dates are typed explicitly.
        private static void AddFieldParameters(OleDbCommand cmd, LeaseAgreement lease)
        {
            cmd.Parameters.AddWithValue("@propertyID", lease.PropertyID);
            cmd.Parameters.AddWithValue("@tenantID", lease.TenantID);
            cmd.Parameters.AddWithValue("@managerID", lease.ManagerID);
            cmd.Parameters.Add("@startDate", OleDbType.Date).Value = lease.LeaseStartDate.Date;
            cmd.Parameters.Add("@endDate", OleDbType.Date).Value = lease.LeaseEndDate.Date;
            cmd.Parameters.Add("@rent", OleDbType.Currency).Value = lease.MonthlyRent;
            cmd.Parameters.AddWithValue("@status", lease.LeaseStatus);
        }

        private static LeaseAgreement Map(OleDbDataReader reader)
        {
            return new LeaseAgreement
            {
                LeaseID = Convert.ToInt32(reader["LeaseID"]),
                PropertyID = reader["PropertyID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PropertyID"]),
                TenantID = reader["TenantID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TenantID"]),
                ManagerID = reader["ManagerID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ManagerID"]),
                LeaseStartDate = reader["LeaseStartDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["LeaseStartDate"]),
                LeaseEndDate = reader["LeaseEndDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["LeaseEndDate"]),
                MonthlyRent = reader["MonthlyRent"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["MonthlyRent"]),
                LeaseStatus = reader["LeaseStatus"].ToString(),
                PropertyAddress = reader["PropertyAddress"].ToString(),
                TenantName = reader["TenantName"].ToString().Trim(),
                ManagerName = reader["ManagerName"].ToString().Trim()
            };
        }
    }
}
