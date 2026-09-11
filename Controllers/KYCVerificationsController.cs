using System;
using System.Web.Mvc;
using AtlasPremierProperties.Models.Entities;
using AtlasPremierProperties.Repositories;

namespace AtlasPremierProperties.Controllers
{
    public class KYCVerificationsController : Controller
    {
        private readonly KYCVerificationRepository _repo = new KYCVerificationRepository();
        private readonly TenantRepository _tenantRepo = new TenantRepository();

        public ActionResult Index()
        {
            return View(_repo.GetAll());
        }

        // Show all KYC records for a specific tenant
        public ActionResult ByTenant(int tenantId)
        {
            ViewBag.TenantId = tenantId;
            return View("Index", _repo.GetByTenantId(tenantId));
        }

        public ActionResult Details(int id)
        {
            var record = _repo.GetById(id);
            if (record == null) return HttpNotFound();
            return View(record);
        }

        public ActionResult Create(int? tenantId)
        {
            // Pre-select tenant if coming from a tenant page
            ViewBag.Tenants = new SelectList(_tenantRepo.GetAll(), "TenantID", "TenantID", tenantId);
            return View(new KYCVerification { TenantID = tenantId ?? 0, VerifiedDate = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KYCVerification model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Tenants = new SelectList(_tenantRepo.GetAll(), "TenantID", "TenantID", model.TenantID);
                return View(model);
            }

            try
            {
                int newId = _repo.Add(model);
                TempData["Success"] = "KYC Verification created. ID = " + newId;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Tenants = new SelectList(_tenantRepo.GetAll(), "TenantID", "TenantID", model.TenantID);
                return View(model);
            }
        }

        public ActionResult Edit(int id)
        {
            var record = _repo.GetById(id);
            if (record == null) return HttpNotFound();

            ViewBag.Tenants = new SelectList(_tenantRepo.GetAll(), "TenantID", "TenantID", record.TenantID);
            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(KYCVerification model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Tenants = new SelectList(_tenantRepo.GetAll(), "TenantID", "TenantID", model.TenantID);
                return View(model);
            }

            _repo.Update(model);
            TempData["Success"] = "KYC Verification updated.";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var record = _repo.GetById(id);
            if (record == null) return HttpNotFound();
            return View(record);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _repo.Delete(id);
            TempData["Success"] = "KYC Verification deleted.";
            return RedirectToAction("Index");
        }
    }
}