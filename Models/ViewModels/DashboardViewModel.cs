using System;
using System.Collections.Generic;

namespace AtlasPremierProperties.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int ActiveLeases { get; set; }
        public int SettlementsThisMonth { get; set; }
        public decimal PayoutsThisMonth { get; set; }
        public int PropertyCount { get; set; }
        public int TenantsPendingKyc { get; set; }

        // Properties with an Active lease that covers today.
        public int OccupiedProperties { get; set; }

        public decimal OccupancyRate
        {
            get { return PropertyCount == 0 ? 0 : Math.Round((decimal)OccupiedProperties / PropertyCount * 100, 1); }
        }

        public List<ActivityItem> RecentActivity { get; set; }

        public DashboardViewModel()
        {
            RecentActivity = new List<ActivityItem>();
        }
    }

    public class ActivityItem
    {
        public string Icon { get; set; }
        public string Text { get; set; }
        public string Meta { get; set; }
        public string Url { get; set; }
    }
}
