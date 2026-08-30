using AtlasPremierProperties.Helpers;
using AtlasPremierProperties.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace AtlasPremierProperties.Repositories
{
    public class OwnerRepository
    {
        private readonly DatabaseHelper _db;

        public OwnerRepository()
        {
            _db = new DatabaseHelper();
        }

        public List<Owner> GetAll()
        {
            var list = new List<Owner>();

           
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                var sql = "SELECT * FROM Owners";

                using (var cmd = new OleDbCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Owner
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

        public Owner GetById(int id)
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
                            return new Owner
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
        public int Add(Owner owner)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                // Check for duplicate email first
                using (var checkCmd = new OleDbCommand(
                    "SELECT COUNT(*) FROM Owners WHERE EmailAddress = ?", conn))
                {
                    checkCmd.Parameters.AddWithValue("@email", owner.EmailAddress);

                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                        throw new Exception("An owner with this email address already exists.");
                }

               
                var sql = "INSERT INTO Owners " +
                          "(FirstName, LastName, EmailAddress, PhoneNumber) " +
                          "VALUES (?, ?, ?, ?)";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@firstName", owner.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", owner.LastName);
                    cmd.Parameters.AddWithValue("@email", owner.EmailAddress);
                    cmd.Parameters.AddWithValue("@phone", owner.PhoneNumber);

                    cmd.ExecuteNonQuery();
                }

                
                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                {
                    return Convert.ToInt32(idCmd.ExecuteScalar());
                }
            }
        }

        public void Update(Owner owner)
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
                    cmd.Parameters.AddWithValue("@firstName", owner.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", owner.LastName);
                    cmd.Parameters.AddWithValue("@email", owner.EmailAddress);
                    cmd.Parameters.AddWithValue("@phone", owner.PhoneNumber);
                    cmd.Parameters.AddWithValue("@id", owner.OwnerID);

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

