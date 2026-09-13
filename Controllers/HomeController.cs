using System;
using System.Linq;
using System.Web.Mvc;
using AtlasPremierProperties.Models.ViewModels;
using AtlasPremierProperties.Repositories;

namespace AtlasPremierProperties.Controllers
{
    public class HomeController : Controller
    {
        private readonly PropertyRepository _properties = new PropertyRepository();
        private readonly TenantRepository _tenants = new TenantRepository();
        private readonly LeaseRepository _leases = new LeaseRepository();
        private readonly SettlementRepository _settlements = new SettlementRepository();
        private readonly OwnersRepository _owners = new OwnersRepository();

        public ActionResult Index()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var properties = _properties.GetAll();
            var leases = _leases.GetAll();
            var settlements = _settlements.GetAll();
            var tenants = _tenants.GetAll();
            var settledThisMonth = settlements.Where(s => s.SettlementDate >= monthStart).ToList();

            var model = new DashboardViewModel
            {
                ActiveLeases = leases.Count(l => l.LeaseStatus == "Active"),
                SettlementsThisMonth = settledThisMonth.Count,
                PayoutsThisMonth = settledThisMonth.Sum(s => s.OwnerPayout),
                PropertyCount = properties.Count,
                TenantsPendingKyc = tenants.Count(t => t.VerificationStatus != "Approved" && t.VerificationStatus != "Declined"),
                OccupiedProperties = leases
                    .Where(l => l.LeaseStatus == "Active" && l.LeaseStartDate <= today && l.LeaseEndDate >= today)
                    .Select(l => l.PropertyID)
                    .Distinct()
                    .Count()
            };

            // Settlements have a date; leases don't record when they were added, so the newest ids come after.
            foreach (var settlement in settlements.OrderByDescending(s => s.SettlementDate).Take(4))
            {
                model.RecentActivity.Add(new ActivityItem
                {
                    Icon = "bitcoin",
                    Text = "Settlement #" + settlement.SettlementID + " for lease #" + settlement.LeaseID + ": owner payout R " + settlement.OwnerPayout.ToString("N2"),
                    Meta = settlement.SettlementDate.ToString("dd MMM yyyy, HH:mm"),
                    Url = Url.Action("Details", "Leases", new { id = settlement.LeaseID })
                });
            }
            foreach (var lease in leases.OrderByDescending(l => l.LeaseID).Take(6 - model.RecentActivity.Count))
            {
                model.RecentActivity.Add(new ActivityItem
                {
                    Icon = "leases",
                    Text = "Lease #" + lease.LeaseID + ": " + lease.TenantName + " at " + lease.PropertyAddress,
                    Meta = lease.LeaseStatus + " · " + lease.LeaseStartDate.ToString("dd MMM yyyy") + " to " + lease.LeaseEndDate.ToString("dd MMM yyyy"),
                    Url = Url.Action("Details", "Leases", new { id = lease.LeaseID })
                });
            }

            return View(model);
        }

        public ActionResult Search(string q)
        {
            var model = new SearchViewModel { Query = (q ?? "").Trim() };
            if (model.Query.Length == 0) return View(model);

            string term = model.Query;
            Func<string, bool> matches = value => value != null && value.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;

            model.Properties = _properties.GetAll()
                .Where(p => matches(p.PhysicalAddress) || matches(p.PropertyDescription) || matches(p.OwnerName))
                .ToList();
            model.Tenants = _tenants.GetAll()
                .Where(t => matches(t.FullName) || matches(t.EmailAddress) || matches(t.PassportIDNumber))
                .ToList();
            model.Leases = _leases.GetAll()
                .Where(l => matches(l.PropertyAddress) || matches(l.TenantName) || matches(l.ManagerName) || "#" + l.LeaseID == term || l.LeaseID.ToString() == term)
                .ToList();
            model.Owners = _owners.GetAll()
                .Where(o => matches(o.FullName) || matches(o.EmailAddress))
                .ToList();

            return View(model);
        }
    }
}
