using System;
using System.Collections.Generic;
using System.Data.OleDb;
using AtlasPremierProperties.Helpers;
using AtlasPremierProperties.Models.Entities;

namespace AtlasPremierProperties.Repositories
{
    public class KYCVerificationRepository
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();

        public List<KYCVerification> GetAll()
        {
            var list = new List<KYCVerification>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                using (var cmd = new OleDbCommand("SELECT * FROM [KYC Verification]", conn))
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

        public List<KYCVerification> GetByTenantId(int tenantId)
        {
            var list = new List<KYCVerification>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                using (var cmd = new OleDbCommand(
                    "SELECT * FROM [KYC Verification] WHERE TenantID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@tid", tenantId);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            list.Add(Map(r));
                    }
                }
            }
            return list;
        }

        public KYCVerification GetById(int id)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                using (var cmd = new OleDbCommand(
                    "SELECT * FROM [KYC Verification] WHERE KYCID = ?", conn))
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

        public int Add(KYCVerification k)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var sql = "INSERT INTO [KYC Verification] " +
                          "(TenantID, DocumentType, VerificationStatus, VerifiedDate) " +
                          "VALUES (?, ?, ?, ?)";
                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@tid", k.TenantID);
                    cmd.Parameters.AddWithValue("@doc", k.DocumentType);
                    cmd.Parameters.AddWithValue("@stat", k.VerificationStatus ?? "Pending");
                    cmd.Parameters.AddWithValue("@date", k.VerifiedDate == default ? DateTime.Now : k.VerifiedDate);
                    cmd.ExecuteNonQuery();
                }
                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                    return Convert.ToInt32(idCmd.ExecuteScalar());
            }
        }

        public void Update(KYCVerification k)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var sql = "UPDATE [KYC Verification] SET " +
                          "TenantID = ?, DocumentType = ?, VerificationStatus = ?, VerifiedDate = ? " +
                          "WHERE KYCID = ?";
                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@tid", k.TenantID);
                    cmd.Parameters.AddWithValue("@doc", k.DocumentType);
                    cmd.Parameters.AddWithValue("@stat", k.VerificationStatus);
                    cmd.Parameters.AddWithValue("@date", k.VerifiedDate);
                    cmd.Parameters.AddWithValue("@id", k.KYCID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                using (var cmd = new OleDbCommand(
                    "DELETE FROM [KYC Verification] WHERE KYCID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private KYCVerification Map(OleDbDataReader r)
        {
            return new KYCVerification
            {
                KYCID = Convert.ToInt32(r["KYCID"]),
                TenantID = Convert.ToInt32(r["TenantID"]),
                DocumentType = r["DocumentType"].ToString(),
                VerificationStatus = r["VerificationStatus"].ToString(),
                VerifiedDate = Convert.ToDateTime(r["VerifiedDate"])
            };
        }
    }
}