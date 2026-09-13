using System;
using System.Linq;
using System.Web.Mvc;
using AtlasPremierProperties.Models.Entities;
using AtlasPremierProperties.Repositories;
using AtlasPremierProperties.Services;

namespace AtlasPremierProperties.Controllers
{
    public class SettlementsController : Controller
    {
        private readonly SettlementRepository _settlementRepo;
        private readonly LeaseRepository _leaseRepo;
        private readonly SettlementService _settlementService;
        private readonly CryptoInvoiceService _cryptoService;

        public SettlementsController()
        {
            _settlementRepo = new SettlementRepository();
            _leaseRepo = new LeaseRepository();
            _settlementService = new SettlementService();
            _cryptoService = new CryptoInvoiceService();
        }

        public ActionResult Index()
        {
            return View(_settlementRepo.GetAll().OrderByDescending(s => s.SettlementDate).ToList());
        }

        // Shows the invoice form with a dropdown of leases and the most recent invoices
        public ActionResult Create()
        {
            PopulateForm(null, "BTC");
            return View();
        }

        // Calculates the settlement from the lease's monthly rent, generates the invoice link, and saves it
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int? leaseId, decimal? maintenanceCosts, int? daysOccupied, string currency)
        {
            var lease = leaseId.HasValue ? _leaseRepo.GetById(leaseId.Value) : null;
            if (lease == null)
            {
                ModelState.AddModelError("", "Select a lease.");
            }
            if (!maintenanceCosts.HasValue || !daysOccupied.HasValue)
            {
                ModelState.AddModelError("", "Enter the maintenance costs and days occupied.");
            }
            if (currency == null || !CryptoInvoiceService.Currencies.ContainsKey(currency))
            {
                ModelState.AddModelError("", "Choose a settlement currency.");
            }
            if (!ModelState.IsValid)
            {
                PopulateForm(leaseId, currency);
                return View();
            }

            try
            {
                var result = _settlementService.Calculate(
                    lease.MonthlyRent, maintenanceCosts.Value, daysOccupied.Value);

                string invoiceLink = _cryptoService.GenerateInvoiceLink(
                    lease.LeaseID, result.OwnerPayout, currency);

                var settlement = new Settlement
                {
                    LeaseID = lease.LeaseID,
                    GrossRent = result.GrossRent,
                    MaintenanceCosts = result.MaintenanceCosts,
                    NetAmount = result.NetAmount,
                    ManagementFee = result.ManagementFee,
                    OwnerPayout = result.OwnerPayout,
                    DaysOccupied = result.DaysOccupied,
                    SettlementDate = DateTime.Now,
                    CryptoInvoiceLink = invoiceLink
                };

                int newId = _settlementRepo.Add(settlement);

                TempData["Success"] =
                    "Settlement ID " + newId + " created successfully.";

                TempData["InvoiceLink"] = invoiceLink;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                PopulateForm(leaseId, currency);
                return View();
            }
        }

        private void PopulateForm(int? leaseId, string currency)
        {
            var leases = _leaseRepo.GetAll();
            var options = leases.Select(l => new
            {
                l.LeaseID,
                Display = "#" + l.LeaseID + " - " + l.PropertyAddress + " (" + l.TenantName + ")"
            });

            ViewBag.Leases = new SelectList(options, "LeaseID", "Display", leaseId);
            ViewBag.LeaseRents = leases.ToDictionary(l => l.LeaseID.ToString(), l => l.MonthlyRent);
            ViewBag.Currencies = new SelectList(CryptoInvoiceService.Currencies, "Key", "Value", currency);
            ViewBag.RecentSettlements = _settlementRepo.GetAll().OrderByDescending(s => s.SettlementDate).Take(5).ToList();
        }
    }
}
