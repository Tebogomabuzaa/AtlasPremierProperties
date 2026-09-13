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
                using (var cmd = new OleDbCommand("SELECT * FROM Tenants", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(Map(r));
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
                using (var cmd = new OleDbCommand("SELECT * FROM Tenants WHERE TenantID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read()) return Map(r);
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
                var sql =
                    "INSERT INTO Tenants " +
                    "(FirstName, LastName, EmailAddress, PassportIDNumber, " +
                    "Nationality, DeclaredMonthlyIncome, VerificationStatus) " +
                    "VALUES (?, ?, ?, ?, ?, ?, ?)";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@firstName", t.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", t.LastName);
                    cmd.Parameters.AddWithValue("@email", t.EmailAddress);
                    cmd.Parameters.AddWithValue("@passport", t.PassportIDNumber);
                    cmd.Parameters.AddWithValue("@nationality", t.Nationality);
                    cmd.Parameters.AddWithValue("@income", t.DeclaredMonthlyIncome);
                    cmd.Parameters.AddWithValue("@status", t.VerificationStatus ?? "Pending");
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
                var sql =
                    "UPDATE Tenants SET " +
                    "FirstName = ?, " +
                    "LastName = ?, " +
                    "EmailAddress = ?, " +
                    "PassportIDNumber = ?, " +
                    "Nationality = ?, " +
                    "DeclaredMonthlyIncome = ?, " +
                    "VerificationStatus = ? " +
                    "WHERE TenantID = ?";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@firstName", t.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", t.LastName);
                    cmd.Parameters.AddWithValue("@email", t.EmailAddress);
                    cmd.Parameters.AddWithValue("@passport", t.PassportIDNumber);
                    cmd.Parameters.AddWithValue("@nationality", t.Nationality);
                    cmd.Parameters.AddWithValue("@income", t.DeclaredMonthlyIncome);
                    cmd.Parameters.AddWithValue("@status", t.VerificationStatus);
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
                using (var cmd = new OleDbCommand("DELETE FROM Tenants WHERE TenantID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private Tenant Map(OleDbDataReader r)
        {
            return new Tenant
            {
                TenantID = Convert.ToInt32(r["TenantID"]),
                FirstName = r["FirstName"].ToString(),
                LastName = r["LastName"].ToString(),
                EmailAddress = r["EmailAddress"].ToString(),
                PassportIDNumber = r["PassportIDNumber"].ToString(),
                Nationality = r["Nationality"].ToString(),
                DeclaredMonthlyIncome = r["DeclaredMonthlyIncome"] == DBNull.Value
                    ? 0m
                    : Convert.ToDecimal(r["DeclaredMonthlyIncome"]),
                VerificationStatus = r["VerificationStatus"].ToString()
            };
        }
    }
}
