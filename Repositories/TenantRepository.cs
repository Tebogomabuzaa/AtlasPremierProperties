using System;
using System.Collections.Generic;
using System.Data.OleDb;
using AtlasPremierProperties.Helpers;
using AtlasPremierProperties.Models.Entities;

namespace AtlasPremierProperties.Repositories
{
    public class TenantRepository
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();

        public List<Tenant> GetAll()
        {
            var list = new List<Tenant>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                using (var cmd = new OleDbCommand("SELECT * FROM Tenant", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new Tenant
                        {
                            TenantID = Convert.ToInt32(r["TenantID"]),
                            UserID = Convert.ToInt32(r["UserID"]),
                            KYCStatus = r["KYCStatus"].ToString(),
                            CreditScore = Convert.ToInt32(r["CreditScore"])
                        });
                    }
                }
            }
            return list;
        }

        public Tenant GetById(int id)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                using (var cmd = new OleDbCommand("SELECT * FROM Tenant WHERE TenantID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            return new Tenant
                            {
                                TenantID = Convert.ToInt32(r["TenantID"]),
                                UserID = Convert.ToInt32(r["UserID"]),
                                KYCStatus = r["KYCStatus"].ToString(),
                                CreditScore = Convert.ToInt32(r["CreditScore"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        public int Add(Tenant t)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var sql = "INSERT INTO Tenant (UserID, KYCStatus, CreditScore) VALUES (?, ?, ?)";
                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", t.UserID);
                    cmd.Parameters.AddWithValue("@kyc", t.KYCStatus ?? "Pending");
                    cmd.Parameters.AddWithValue("@cs", t.CreditScore);
                    cmd.ExecuteNonQuery();
                }
                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                    return Convert.ToInt32(idCmd.ExecuteScalar());
            }
        }

        public void Update(Tenant t)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var sql = "UPDATE Tenant SET UserID = ?, KYCStatus = ?, CreditScore = ? WHERE TenantID = ?";
                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", t.UserID);
                    cmd.Parameters.AddWithValue("@kyc", t.KYCStatus);
                    cmd.Parameters.AddWithValue("@cs", t.CreditScore);
                    cmd.Parameters.AddWithValue("@id", t.TenantID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                using (var cmd = new OleDbCommand("DELETE FROM Tenant WHERE TenantID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}