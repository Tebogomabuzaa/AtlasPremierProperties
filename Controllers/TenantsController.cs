using System.Web.Mvc;
using AtlasPremierProperties.Models.Entities;
using AtlasPremierProperties.Repositories;

namespace AtlasPremierProperties.Controllers
{
    public class TenantsController : Controller
    {
        private readonly TenantRepository _repo = new TenantRepository();

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
                int newId = _repo.Add(tenant);
                TempData["Success"] = "Tenant created. ID = " + newId;
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
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

            _repo.Update(tenant);
            TempData["Success"] = "Tenant updated successfully.";
            return RedirectToAction("Index");
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
            _repo.Delete(id);
            TempData["Success"] = "Tenant deleted.";
            return RedirectToAction("Index");
        }
    }
}