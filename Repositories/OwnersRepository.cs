using System;
using System.Collections.Generic;
using System.Data.OleDb;
using AtlasPremierProperties.Helpers;
using AtlasPremierProperties.Models.Entities;

namespace AtlasPremierProperties.Repositories
{
    public class OwnersRepository
    {
        private readonly DatabaseHelper _db;

        public OwnersRepository()
        {
            _db = new DatabaseHelper();
        }

        public List<Owners> GetAll()
        {
            var list = new List<Owners>();

            using (var conn = _db.GetConnection())
            {
                conn.Open();

                using (var cmd = new OleDbCommand("SELECT * FROM Owners ORDER BY LastName, FirstName", conn))
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

        public Owners GetById(int id)
        {
            return GetSingle("SELECT * FROM Owners WHERE OwnerID = ?", id);
        }

        public Owners GetByEmail(string email)
        {
            return GetSingle("SELECT * FROM Owners WHERE EmailAddress = ?", email);
        }

        // Returns the new OwnerID
        public int Add(Owners owners)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                if (EmailInUse(conn, owners.EmailAddress, null))
                    throw new Exception("An owner with this email address already exists.");

                var sql = "INSERT INTO Owners " +
                          "(FirstName, LastName, EmailAddress, PhoneNumber, PasswordHash) " +
                          "VALUES (?, ?, ?, ?, ?)";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@firstName", owners.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", owners.LastName);
                    cmd.Parameters.AddWithValue("@email", owners.EmailAddress);
                    cmd.Parameters.AddWithValue("@phone", DatabaseHelper.ToDbValue(owners.PhoneNumber));
                    cmd.Parameters.AddWithValue("@passwordHash", DatabaseHelper.ToDbValue(owners.PasswordHash));

                    cmd.ExecuteNonQuery();
                }

                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                {
                    return Convert.ToInt32(idCmd.ExecuteScalar());
                }
            }
        }

        // Leaves the stored password hash unchanged when owners.PasswordHash is null or empty.
        public void Update(Owners owners)
        {
            bool changePassword = !string.IsNullOrEmpty(owners.PasswordHash);

            using (var conn = _db.GetConnection())
            {
                conn.Open();

                if (EmailInUse(conn, owners.EmailAddress, owners.OwnerID))
                    throw new Exception("Another owner already uses this email address.");

                var sql = "UPDATE Owners SET " +
                          "FirstName = ?, " +
                          "LastName = ?, " +
                          "EmailAddress = ?, " +
                          "PhoneNumber = ?" +
                          (changePassword ? ", PasswordHash = ? " : " ") +
                          "WHERE OwnerID = ?";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@firstName", owners.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", owners.LastName);
                    cmd.Parameters.AddWithValue("@email", owners.EmailAddress);
                    cmd.Parameters.AddWithValue("@phone", DatabaseHelper.ToDbValue(owners.PhoneNumber));
                    if (changePassword)
                    {
                        cmd.Parameters.AddWithValue("@passwordHash", owners.PasswordHash);
                    }
                    cmd.Parameters.AddWithValue("@id", owners.OwnerID);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int ownerId)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                using (var checkCmd = new OleDbCommand(
                    "SELECT COUNT(*) FROM Properties WHERE OwnerID = ?", conn))
                {
                    checkCmd.Parameters.AddWithValue("@id", ownerId);

                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                        throw new Exception("This owner still has properties linked and cannot be deleted.");
                }

                using (var cmd = new OleDbCommand(
                    "DELETE FROM Owners WHERE OwnerID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", ownerId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static bool EmailInUse(OleDbConnection conn, string email, int? excludeOwnerId)
        {
            var sql = "SELECT COUNT(*) FROM Owners WHERE EmailAddress = ?";
            if (excludeOwnerId.HasValue) sql += " AND OwnerID <> ?";

            using (var cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@email", email);
                if (excludeOwnerId.HasValue) cmd.Parameters.AddWithValue("@id", excludeOwnerId.Value);

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private Owners GetSingle(string sql, object parameter)
        {
            using (var conn = _db.GetConnection())
            using (var cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@p", parameter);
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        private static Owners Map(OleDbDataReader reader)
        {
            return new Owners
            {
                OwnerID = Convert.ToInt32(reader["OwnerID"]),
                FirstName = reader["FirstName"].ToString(),
                LastName = reader["LastName"].ToString(),
                EmailAddress = reader["EmailAddress"].ToString(),
                PhoneNumber = reader["PhoneNumber"].ToString(),
                PasswordHash = reader["PasswordHash"] == DBNull.Value ? null : reader["PasswordHash"].ToString()
            };
        }
    }
}
