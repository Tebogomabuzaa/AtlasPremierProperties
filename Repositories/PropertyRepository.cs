using System;
using System.Collections.Generic;
using System.Data.OleDb;
using AtlasPremierProperties.Helpers;
using AtlasPremierProperties.Models.Entities;

namespace AtlasPremierProperties.Repositories
{
    public class PropertyRepository
    {
        private const string SelectWithOwner =
            "SELECT p.PropertyID, p.PropertyDescription, p.PhysicalAddress, p.OwnerID, p.MonthlyRent, p.VacancyStatus, " +
            "o.FirstName & ' ' & o.LastName AS OwnerName " +
            "FROM Properties AS p LEFT JOIN Owners AS o ON p.OwnerID = o.OwnerID ";

        private readonly DatabaseHelper _db = new DatabaseHelper();

        public List<Property> GetAll()
        {
            return Query(SelectWithOwner + "ORDER BY p.PhysicalAddress");
        }

        public Property GetById(int id)
        {
            var list = Query(SelectWithOwner + "WHERE p.PropertyID = ?", id);
            return list.Count > 0 ? list[0] : null;
        }

        public List<Property> GetByOwner(int ownerId)
        {
            return Query(SelectWithOwner + "WHERE p.OwnerID = ? ORDER BY p.PhysicalAddress", ownerId);
        }

        public bool AddressExists(string address, int? excludePropertyId = null)
        {
            string sql = "SELECT COUNT(*) FROM Properties WHERE PhysicalAddress = ?";
            if (excludePropertyId.HasValue) sql += " AND PropertyID <> ?";

            using (var conn = _db.GetConnection())
            using (var cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@address", address);
                if (excludePropertyId.HasValue) cmd.Parameters.AddWithValue("@id", excludePropertyId.Value);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public bool HasLeases(int propertyId)
        {
            using (var conn = _db.GetConnection())
            using (var cmd = new OleDbCommand("SELECT COUNT(*) FROM LeaseAgreements WHERE PropertyID = ?", conn))
            {
                cmd.Parameters.AddWithValue("@id", propertyId);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public int Add(Property property)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                var sql =
                    "INSERT INTO Properties " +
                    "(PropertyDescription, PhysicalAddress, OwnerID, MonthlyRent, VacancyStatus) " +
                    "VALUES (?, ?, ?, ?, ?)";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@description", DatabaseHelper.ToDbValue(property.PropertyDescription));
                    cmd.Parameters.AddWithValue("@address", property.PhysicalAddress);
                    cmd.Parameters.AddWithValue("@ownerId", property.OwnerID);
                    cmd.Parameters.Add("@rent", OleDbType.Currency).Value = property.MonthlyRent;
                    cmd.Parameters.AddWithValue("@vacant", property.VacancyStatus);
                    cmd.ExecuteNonQuery();
                }

                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                {
                    return Convert.ToInt32(idCmd.ExecuteScalar());
                }
            }
        }

        public void Update(Property property)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                var sql =
                    "UPDATE Properties SET " +
                    "PropertyDescription = ?, " +
                    "PhysicalAddress = ?, " +
                    "OwnerID = ?, " +
                    "MonthlyRent = ?, " +
                    "VacancyStatus = ? " +
                    "WHERE PropertyID = ?";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@description", DatabaseHelper.ToDbValue(property.PropertyDescription));
                    cmd.Parameters.AddWithValue("@address", property.PhysicalAddress);
                    cmd.Parameters.AddWithValue("@ownerId", property.OwnerID);
                    cmd.Parameters.Add("@rent", OleDbType.Currency).Value = property.MonthlyRent;
                    cmd.Parameters.AddWithValue("@vacant", property.VacancyStatus);
                    cmd.Parameters.AddWithValue("@id", property.PropertyID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int propertyId)
        {
            using (var conn = _db.GetConnection())
            using (var cmd = new OleDbCommand("DELETE FROM Properties WHERE PropertyID = ?", conn))
            {
                cmd.Parameters.AddWithValue("@id", propertyId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private List<Property> Query(string sql, params object[] parameters)
        {
            var list = new List<Property>();

            using (var conn = _db.GetConnection())
            using (var cmd = new OleDbCommand(sql, conn))
            {
                foreach (var value in parameters)
                {
                    cmd.Parameters.AddWithValue("?", value);
                }

                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new Property
                        {
                            PropertyID = Convert.ToInt32(r["PropertyID"]),
                            PropertyDescription = r["PropertyDescription"].ToString(),
                            PhysicalAddress = r["PhysicalAddress"].ToString(),
                            OwnerID = r["OwnerID"] == DBNull.Value ? 0 : Convert.ToInt32(r["OwnerID"]),
                            MonthlyRent = r["MonthlyRent"] == DBNull.Value ? 0m : Convert.ToDecimal(r["MonthlyRent"]),
                            VacancyStatus = Convert.ToBoolean(r["VacancyStatus"]),
                            OwnerName = r["OwnerName"].ToString().Trim()
                        });
                    }
                }
            }

            return list;
        }
    }
}
