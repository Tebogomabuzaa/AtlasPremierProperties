using System;
using System.Collections.Generic;
using System.Data.OleDb;
using AtlasPremierProperties.Helpers;
using AtlasPremierProperties.Models.Entities;

namespace AtlasPremierProperties.Repositories
{
    public class LeaseRepository
    {
        private readonly DatabaseHelper _db;

        public LeaseRepository()
        {
            _db = new DatabaseHelper();
        }

        public List<LeaseAgreement> GetAll()
        {
            var list = new List<LeaseAgreement>();

            using (var conn = _db.GetConnection())
            {
                conn.Open();

                var sql =
                    "SELECT la.*, " +
                    "p.PhysicalAddress AS PropertyAddress, " +
                    "t.FirstName & ' ' & t.LastName AS TenantName, " +
                    "pm.FirstName & ' ' & pm.LastName AS ManagerName " +
                    "FROM (LeaseAgreements la " +
                    "LEFT JOIN Properties p ON la.PropertyID = p.PropertyID) " +
                    "LEFT JOIN Tenants t ON la.TenantID = t.TenantID " +
                    "LEFT JOIN PropertyManagers pm ON la.ManagerID = pm.ManagerID";

                using (var cmd = new OleDbCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new LeaseAgreement
                        {
                            LeaseID = Convert.ToInt32(reader["LeaseID"]),
                            PropertyID = Convert.ToInt32(reader["PropertyID"]),
                            TenantID = Convert.ToInt32(reader["TenantID"]),
                            ManagerID = Convert.ToInt32(reader["ManagerID"]),
                            LeaseStartDate = Convert.ToDateTime(reader["LeaseStartDate"]),
                            LeaseEndDate = Convert.ToDateTime(reader["LeaseEndDate"]),
                            MonthlyRent = Convert.ToDecimal(reader["MonthlyRent"]),
                            LeaseStatus = reader["LeaseStatus"].ToString(),
                            PropertyAddress = reader["PropertyAddress"].ToString(),
                            TenantName = reader["TenantName"].ToString(),
                            ManagerName = reader["ManagerName"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        public LeaseAgreement GetById(int id)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                var sql =
                    "SELECT la.*, " +
                    "p.PhysicalAddress AS PropertyAddress, " +
                    "t.FirstName & ' ' & t.LastName AS TenantName, " +
                    "pm.FirstName & ' ' & pm.LastName AS ManagerName " +
                    "FROM (LeaseAgreements la " +
                    "LEFT JOIN Properties p ON la.PropertyID = p.PropertyID) " +
                    "LEFT JOIN Tenants t ON la.TenantID = t.TenantID " +
                    "LEFT JOIN PropertyManagers pm ON la.ManagerID = pm.ManagerID " +
                    "WHERE la.LeaseID = ?";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new LeaseAgreement
                            {
                                LeaseID = Convert.ToInt32(reader["LeaseID"]),
                                PropertyID = Convert.ToInt32(reader["PropertyID"]),
                                TenantID = Convert.ToInt32(reader["TenantID"]),
                                ManagerID = Convert.ToInt32(reader["ManagerID"]),
                                LeaseStartDate = Convert.ToDateTime(reader["LeaseStartDate"]),
                                LeaseEndDate = Convert.ToDateTime(reader["LeaseEndDate"]),
                                MonthlyRent = Convert.ToDecimal(reader["MonthlyRent"]),
                                LeaseStatus = reader["LeaseStatus"].ToString(),
                                PropertyAddress = reader["PropertyAddress"].ToString(),
                                TenantName = reader["TenantName"].ToString(),
                                ManagerName = reader["ManagerName"].ToString()
                            };
                        }
                    }
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
                    cmd.Parameters.AddWithValue("@propertyID", lease.PropertyID);
                    cmd.Parameters.AddWithValue("@tenantID", lease.TenantID);
                    cmd.Parameters.AddWithValue("@managerID", lease.ManagerID);
                    cmd.Parameters.AddWithValue("@startDate", lease.LeaseStartDate);
                    cmd.Parameters.AddWithValue("@endDate", lease.LeaseEndDate);
                    cmd.Parameters.AddWithValue("@rent", lease.MonthlyRent);
                    cmd.Parameters.AddWithValue("@status", lease.LeaseStatus);

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
                    cmd.Parameters.AddWithValue("@propertyID", lease.PropertyID);
                    cmd.Parameters.AddWithValue("@tenantID", lease.TenantID);
                    cmd.Parameters.AddWithValue("@managerID", lease.ManagerID);
                    cmd.Parameters.AddWithValue("@startDate", lease.LeaseStartDate);
                    cmd.Parameters.AddWithValue("@endDate", lease.LeaseEndDate);
                    cmd.Parameters.AddWithValue("@rent", lease.MonthlyRent);
                    cmd.Parameters.AddWithValue("@status", lease.LeaseStatus);
                    cmd.Parameters.AddWithValue("@id", lease.LeaseID);

                    cmd.ExecuteNonQuery();
                }
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
    }


}
