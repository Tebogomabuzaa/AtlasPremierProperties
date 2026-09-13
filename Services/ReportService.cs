using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
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

        // Occupancy = days each manager's leases overlap the period, divided by the days in the period.
        public List<CoHostStats> GetCoHostPerformance(
            DateTime start,
            DateTime end)
        {
            start = start.Date;
            end = end.Date;
            int totalDays = (end - start).Days + 1;

            var byManager = new Dictionary<int, CoHostStats>();

            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string sql =
                     "SELECT pm.ManagerID, pm.FirstName & ' ' & pm.LastName AS ManagerName, " +
                     "la.LeaseStartDate, la.LeaseEndDate " +
                     "FROM LeaseAgreements AS la " +
                     "INNER JOIN PropertyManagers AS pm ON la.ManagerID = pm.ManagerID " +
                     "WHERE la.LeaseStartDate <= ? AND la.LeaseEndDate >= ?";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@end", end);
                    cmd.Parameters.AddWithValue("@start", start);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int managerId = Convert.ToInt32(reader["ManagerID"]);
                            DateTime leaseStart = Convert.ToDateTime(reader["LeaseStartDate"]).Date;
                            DateTime leaseEnd = Convert.ToDateTime(reader["LeaseEndDate"]).Date;

                            DateTime overlapStart = leaseStart > start ? leaseStart : start;
                            DateTime overlapEnd = leaseEnd < end ? leaseEnd : end;

                            CoHostStats stats;
                            if (!byManager.TryGetValue(managerId, out stats))
                            {
                                stats = new CoHostStats
                                {
                                    ManagerName = reader["ManagerName"].ToString(),
                                    TotalDays = totalDays
                                };
                                byManager.Add(managerId, stats);
                            }

                            stats.DaysOccupied += (overlapEnd - overlapStart).Days + 1;
                        }
                    }
                }
            }

            // A manager with several leases can exceed the period length, so cap occupancy at 100%.
            foreach (var stats in byManager.Values)
            {
                stats.DaysOccupied = Math.Min(stats.DaysOccupied, totalDays);
            }

            return byManager.Values.OrderByDescending(s => s.OccupancyRate).ToList();
        }
    }
}
