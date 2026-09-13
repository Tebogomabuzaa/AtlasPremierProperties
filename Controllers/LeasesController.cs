using System;
using System.Data.OleDb;
using System.Linq;
using System.Web.Mvc;
using AtlasPremierProperties.Models.Entities;
using AtlasPremierProperties.Repositories;

namespace AtlasPremierProperties.Controllers
{
    public class LeasesController : Controller
    {
        private readonly LeaseRepository _leases = new LeaseRepository();
        private readonly PropertyRepository _properties = new PropertyRepository();
        private readonly TenantRepository _tenants = new TenantRepository();
        private readonly PropertyManagerRepository _managers = new PropertyManagerRepository();
        private readonly SettlementRepository _settlements = new SettlementRepository();

        public ActionResult Index()
        {
            return View(_leases.GetAll());
        }

        public ActionResult Details(int id)
        {
            var lease = _leases.GetById(id);
            if (lease == null) return HttpNotFound();

            ViewBag.Settlements = _settlements.GetAll().Where(s => s.LeaseID == id).ToList();
            return View(lease);
        }

        public ActionResult Create()
        {
            var lease = new LeaseAgreement
            {
                LeaseStartDate = DateTime.Today,
                LeaseEndDate = DateTime.Today.AddYears(1).AddDays(-1),
                LeaseStatus = "Active"
            };

            PopulateLists(lease);
            return View(lease);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "LeaseID")] LeaseAgreement lease)
        {
            ValidateLease(lease, null);

            if (ModelState.IsValid)
            {
                try
                {
                    int newId = _leases.Add(lease);
                    TempData["Success"] = "Lease ID " + newId + " created.";
                    return RedirectToAction("Index");
                }
                catch (OleDbException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            PopulateLists(lease);
            return View(lease);
        }

        public ActionResult Edit(int id)
        {
            var lease = _leases.GetById(id);
            if (lease == null) return HttpNotFound();

            PopulateLists(lease);
            return View(lease);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LeaseAgreement lease)
        {
            var existing = _leases.GetById(lease.LeaseID);
            if (existing == null) return HttpNotFound();

            ValidateLease(lease, existing);

            if (ModelState.IsValid)
            {
                try
                {
                    _leases.Update(lease);
                    TempData["Success"] = "Lease ID " + lease.LeaseID + " updated.";
                    return RedirectToAction("Index");
                }
                catch (OleDbException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            PopulateLists(lease);
            return View(lease);
        }

        public ActionResult Delete(int id)
        {
            var lease = _leases.GetById(id);
            if (lease == null) return HttpNotFound();

            ViewBag.HasSettlements = _leases.HasSettlements(id);
            return View(lease);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (_leases.HasSettlements(id))
            {
                TempData["Error"] = "Lease ID " + id + " has settlements linked and cannot be deleted. Set its status to Ended or Terminated instead.";
                return RedirectToAction("Index");
            }

            _leases.Delete(id);
            TempData["Success"] = "Lease ID " + id + " deleted.";
            return RedirectToAction("Index");
        }

        // Business rules the database can't enforce. existing is null when a lease is being created.
        private void ValidateLease(LeaseAgreement lease, LeaseAgreement existing)
        {
            if (!ModelState.IsValid) return;

            if (lease.LeaseEndDate <= lease.LeaseStartDate)
            {
                ModelState.AddModelError("LeaseEndDate", "End date must be after the start date.");
            }

            if (!LeaseAgreement.Statuses.Contains(lease.LeaseStatus))
            {
                ModelState.AddModelError("LeaseStatus", "Choose a valid status.");
            }

            if (_properties.GetById(lease.PropertyID) == null)
            {
                ModelState.AddModelError("PropertyID", "Select a property.");
            }

            if (_managers.GetById(lease.ManagerID) == null)
            {
                ModelState.AddModelError("ManagerID", "Select a property manager.");
            }

            // Only tenants who passed KYC can be put on a lease. Editing an existing lease keeps its tenant allowed.
            var tenant = _tenants.GetById(lease.TenantID);
            if (tenant == null)
            {
                ModelState.AddModelError("TenantID", "Select a tenant.");
            }
            else if (tenant.VerificationStatus != "Approved" && (existing == null || existing.TenantID != lease.TenantID))
            {
                ModelState.AddModelError("TenantID", tenant.FullName + " has not passed KYC. Run KYC on the Tenants page first.");
            }

            if (ModelState.IsValid && lease.IsOpen)
            {
                bool clash = _leases.GetAll().Any(l =>
                    l.PropertyID == lease.PropertyID &&
                    l.LeaseID != lease.LeaseID &&
                    l.IsOpen &&
                    l.LeaseStartDate <= lease.LeaseEndDate &&
                    lease.LeaseStartDate <= l.LeaseEndDate);

                if (clash)
                {
                    ModelState.AddModelError("PropertyID", "This property already has a pending or active lease that overlaps these dates.");
                }
            }
        }

        private void PopulateLists(LeaseAgreement lease)
        {
            var properties = _properties.GetAll();
            ViewBag.Properties = new SelectList(properties, "PropertyID", "PhysicalAddress", lease.PropertyID);
            ViewBag.PropertyRents = properties.ToDictionary(p => p.PropertyID.ToString(), p => p.MonthlyRent);

            var tenants = _tenants.GetAll().Select(t => new
            {
                t.TenantID,
                Display = t.FullName + " (KYC: " + (string.IsNullOrEmpty(t.VerificationStatus) ? "Pending" : t.VerificationStatus) + ")"
            });
            ViewBag.Tenants = new SelectList(tenants, "TenantID", "Display", lease.TenantID);

            ViewBag.Managers = new SelectList(_managers.GetAll(), "ManagerID", "FullName", lease.ManagerID);
            ViewBag.Statuses = new SelectList(LeaseAgreement.Statuses, lease.LeaseStatus);
        }
    }
}
