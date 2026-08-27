using System;
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
            return View(_settlementRepo.GetAll());
        }

        // Shows the Create form with a dropdown of leases
        public ActionResult Create()
        {
            ViewBag.Leases = new SelectList(
                _leaseRepo.GetAll(), "LeaseID", "PropertyAddress");

            return View();
        }

        // Calculates the settlement, generates the invoice link, and saves it
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            int leaseId,
            decimal grossRent,
            decimal maintenanceCosts,
            int daysOccupied)
        {
            try
            {
                var result = _settlementService.Calculate(
                    grossRent, maintenanceCosts, daysOccupied);

                string invoiceLink = _cryptoService.GenerateInvoiceLink(
                    leaseId, result.OwnerPayout);

                var settlement = new Settlement
                {
                    LeaseID = leaseId,
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

                ViewBag.Leases = new SelectList(
                    _leaseRepo.GetAll(), "LeaseID", "PropertyAddress");

                return View();
            }
        }
    }
}