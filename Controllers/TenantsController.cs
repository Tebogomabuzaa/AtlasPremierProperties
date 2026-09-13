using System;
using System.Web.Mvc;
using AtlasPremierProperties.Models.Entities;
using AtlasPremierProperties.Repositories;
using AtlasPremierProperties.Services;

namespace AtlasPremierProperties.Controllers
{
    public class TenantsController : Controller
    {
        private readonly TenantRepository _repo = new TenantRepository();
        private readonly KycService _kycService = new KycService();

        public ActionResult Index()
        {
            return View(_repo.GetAll());
        }

        public ActionResult Details(int id)
        {
            var tenant = _repo.GetById(id);
            if (tenant == null) return HttpNotFound();
            return View(tenant);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Tenant tenant)
        {
            if (!ModelState.IsValid) return View(tenant);

            try
            {
                tenant.VerificationStatus = "Pending";
                int newId = _repo.Add(tenant);
                TempData["Success"] = "Tenant created. ID = " + newId;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(tenant);
            }
        }

        public ActionResult Edit(int id)
        {
            var tenant = _repo.GetById(id);
            if (tenant == null) return HttpNotFound();
            return View(tenant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Tenant tenant)
        {
            if (!ModelState.IsValid) return View(tenant);

            try
            {
                _repo.Update(tenant);
                TempData["Success"] = "Tenant updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(tenant);
            }
        }

        public ActionResult Delete(int id)
        {
            var tenant = _repo.GetById(id);
            if (tenant == null) return HttpNotFound();
            return View(tenant);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (_repo.HasLeases(id))
            {
                TempData["Error"] = "Tenant ID " + id + " has lease agreements linked and cannot be deleted.";
                return RedirectToAction("Index");
            }

            _repo.Delete(id);
            TempData["Success"] = "Tenant deleted.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RunKyc(int id)
        {
            var tenant = _repo.GetById(id);
            if (tenant == null) return HttpNotFound();

            var result = _kycService.VerifyTenant(tenant);
            _repo.UpdateVerificationStatus(id, result.Status);

            TempData["KycResult"] = result.Status;
            TempData["KycMessage"] = tenant.FullName + ": " + result.Message;
            return RedirectToAction("Index");
        }
    }
}
