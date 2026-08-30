using AtlasPremierProperties.Helpers;
using AtlasPremierProperties.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data.OleDb;

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

                var sql = "SELECT * FROM Owners";

                using (var cmd = new OleDbCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Owners
                        {
                            OwnerID = Convert.ToInt32(reader["OwnerID"]),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            EmailAddress = reader["EmailAddress"].ToString(),
                            PhoneNumber = reader["PhoneNumber"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        public Owners GetById(int id)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                using (var cmd = new OleDbCommand(
                    "SELECT * FROM Owners WHERE OwnerID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Owners
                            {
                                OwnerID = Convert.ToInt32(reader["OwnerID"]),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                EmailAddress = reader["EmailAddress"].ToString(),
                                PhoneNumber = reader["PhoneNumber"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }

        // Returns the new OwnerID
        public int Add(Owners owners)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                // Check for duplicate email first
                using (var checkCmd = new OleDbCommand(
                    "SELECT COUNT(*) FROM Owners WHERE EmailAddress = ?", conn))
                {
                    checkCmd.Parameters.AddWithValue("@email", owners.EmailAddress);

                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                        throw new Exception("An owner with this email address already exists.");
                }

               
                var sql = "INSERT INTO Owners " +
                          "(FirstName, LastName, EmailAddress, PhoneNumber) " +
                          "VALUES (?, ?, ?, ?)";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@firstName", owners.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", owners.LastName);
                    cmd.Parameters.AddWithValue("@email", owners.EmailAddress);
                    cmd.Parameters.AddWithValue("@phone", owners.PhoneNumber);

                    cmd.ExecuteNonQuery();
                }

                
                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                {
                    return Convert.ToInt32(idCmd.ExecuteScalar());
                }
            }
        }

        public void Update(Owners owners)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                var sql = "UPDATE Owners SET " +
                          "FirstName = ?, " +
                          "LastName = ?, " +
                          "EmailAddress = ?, " +
                          "PhoneNumber = ? " +
                          "WHERE OwnerID = ?";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@firstName", owners.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", owners.LastName);
                    cmd.Parameters.AddWithValue("@email", owners.EmailAddress);
                    cmd.Parameters.AddWithValue("@phone", owners.PhoneNumber);
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
    }
}        

