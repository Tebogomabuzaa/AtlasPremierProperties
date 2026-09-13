using System;
using System.Data.OleDb;
using AtlasPremierProperties.Helpers;
using AtlasPremierProperties.Models.Entities;

namespace AtlasPremierProperties.Repositories
{
    public class SystemUserRepository
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();

        public bool Any()
        {
            using (var conn = _db.GetConnection())
            using (var cmd = new OleDbCommand("SELECT COUNT(*) FROM SystemUsers", conn))
            {
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public SystemUser GetByUsername(string username)
        {
            using (var conn = _db.GetConnection())
            using (var cmd = new OleDbCommand(
                "SELECT UserID, Username, PasswordHash, UserRole FROM SystemUsers WHERE Username = ?", conn))
            {
                cmd.Parameters.AddWithValue("@username", username);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return null;

                    return new SystemUser
                    {
                        UserID = Convert.ToInt32(r["UserID"]),
                        Username = r["Username"].ToString(),
                        PasswordHash = r["PasswordHash"].ToString(),
                        UserRole = r["UserRole"].ToString()
                    };
                }
            }
        }

        // Returns the new UserID
        public int Add(string username, string passwordHash, string role)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                using (var cmd = new OleDbCommand(
                    "INSERT INTO SystemUsers (Username, PasswordHash, UserRole) VALUES (?, ?, ?)", conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@hash", passwordHash);
                    cmd.Parameters.AddWithValue("@role", role);
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
