using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using AtlasPremierProperties.Helpers;
using AtlasPremierProperties.Models.Entities;

namespace AtlasPremierProperties.Repositories
{
    public class PropertyManagerRepository : IPropertyManagerRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public PropertyManagerRepository()
        {
            _dbHelper = new DatabaseHelper();
        }

        public PropertyManagerRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public List<PropertyManager> GetAll()
        {
            var managers = new List<PropertyManager>();

            using (var conn = _dbHelper.GetConnection())
            using (var cmd = new OleDbCommand(
                "SELECT ManagerID, FirstName, LastName, EmailAddress, PhoneNumber, DateHired " +
                "FROM PropertyManagers ORDER BY LastName, FirstName", conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        managers.Add(MapReaderToManager(reader));
                    }
                }
            }

            return managers;
        }

        public PropertyManager GetById(int managerId)
        {
            using (var conn = _dbHelper.GetConnection())
            using (var cmd = new OleDbCommand(
                "SELECT ManagerID, FirstName, LastName, EmailAddress, PhoneNumber, DateHired " +
                "FROM PropertyManagers WHERE ManagerID = ?", conn))
            {
                cmd.Parameters.AddWithValue("@ManagerID", managerId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapReaderToManager(reader);
                    }
                }
            }

            return null;
        }

        public bool EmailExists(string email, int? excludeManagerId = null)
        {
            string sql = "SELECT COUNT(*) FROM PropertyManagers WHERE EmailAddress = ?";
            if (excludeManagerId.HasValue)
            {
                sql += " AND ManagerID <> ?";
            }

            using (var conn = _dbHelper.GetConnection())
            using (var cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                if (excludeManagerId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@ExcludeManagerID", excludeManagerId.Value);
                }

                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public int Add(PropertyManager manager)
        {
            using (var conn = _dbHelper.GetConnection())
            using (var cmd = new OleDbCommand(
                "INSERT INTO PropertyManagers (FirstName, LastName, EmailAddress, PhoneNumber, DateHired) " +
                "VALUES (?, ?, ?, ?, ?)", conn))
            {
                cmd.Parameters.AddWithValue("@FirstName", manager.FirstName);
                cmd.Parameters.AddWithValue("@LastName", manager.LastName);
                cmd.Parameters.AddWithValue("@Email", manager.Email);
                cmd.Parameters.AddWithValue("@PhoneNumber", manager.PhoneNumber);
                cmd.Parameters.AddWithValue("@DateHired", manager.DateHired);

                conn.Open();
                cmd.ExecuteNonQuery();

                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                {
                    return Convert.ToInt32(idCmd.ExecuteScalar());
                }
            }
        }

        public void Update(PropertyManager manager)
        {
            using (var conn = _dbHelper.GetConnection())
            using (var cmd = new OleDbCommand(
                "UPDATE PropertyManagers SET FirstName = ?, LastName = ?, EmailAddress = ?, " +
                "PhoneNumber = ?, DateHired = ? WHERE ManagerID = ?", conn))
            {
                cmd.Parameters.AddWithValue("@FirstName", manager.FirstName);
                cmd.Parameters.AddWithValue("@LastName", manager.LastName);
                cmd.Parameters.AddWithValue("@Email", manager.Email);
                cmd.Parameters.AddWithValue("@PhoneNumber", manager.PhoneNumber);
                cmd.Parameters.AddWithValue("@DateHired", manager.DateHired);
                cmd.Parameters.AddWithValue("@ManagerID", manager.ManagerID);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int managerId)
        {
            using (var conn = _dbHelper.GetConnection())
            using (var cmd = new OleDbCommand(
                "DELETE FROM PropertyManagers WHERE ManagerID = ?", conn))
            {
                cmd.Parameters.AddWithValue("@ManagerID", managerId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static PropertyManager MapReaderToManager(IDataRecord reader)
        {
            // PhoneNumber and DateHired were added after the table existed, so rows edited directly in Access may have nulls.
            return new PropertyManager
            {
                ManagerID = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Email = reader.GetString(3),
                PhoneNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                DateHired = reader.IsDBNull(5) ? DateTime.MinValue : reader.GetDateTime(5)
            };
        }
    }
}
