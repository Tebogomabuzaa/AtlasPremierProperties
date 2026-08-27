using System;
using System.Collections.Generic;
using System.Data.OleDb;
using AtlasPremierProperties.Helpers;

namespace AtlasPremierProperties.Services
{
    public class CoHostStats
    {
        public string ManagerName { get; set; }
        public int TotalDays { get; set; }
        public int DaysOccupied { get; set; }

        public decimal OccupancyRate
        {
            get
            {
                return TotalDays > 0
                    ? Math.Round((decimal)DaysOccupied / TotalDays * 100, 1)
                    : 0;
            }
        }
    }

    public class ReportService
    {
        private readonly DatabaseHelper _db;

        public ReportService()
        {
            _db = new DatabaseHelper();
        }

        public List<CoHostStats> GetCoHostPerformance(
            DateTime start,
            DateTime end)
        {
            var stats = new List<CoHostStats>();

            int totalDays = (end - start).Days + 1;

            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string sql =
                    "SELECT pm.FirstName & ' ' & pm.LastName AS ManagerName, " +
                    "COUNT(*) AS LeaseDays " +
                    "FROM LeaseAgreements la " +
                    "JOIN PropertyManagers pm ON la.ManagerID = pm.ManagerID " +
                    "WHERE la.LeaseStartDate <= ? AND la.LeaseEndDate >= ? " +
                    "GROUP BY pm.FirstName & ' ' & pm.LastName";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@end", end);
                    cmd.Parameters.AddWithValue("@start", start);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stats.Add(new CoHostStats
                            {
                                ManagerName = reader["ManagerName"].ToString(),
                                TotalDays = totalDays,
                                DaysOccupied = Convert.ToInt32(reader["LeaseDays"])
                            });
                        }
                    }
                }
            }

            return stats;
        }
    }
}