using System.Web.Mvc;
using AtlasPremierProperties.Models.Entities;
using AtlasPremierProperties.Repositories;

namespace AtlasPremierProperties.Controllers
{
    public class LeasesController : Controller
    {
        private readonly LeaseRepository _leaseRepository;

        public LeasesController()
        {
            _leaseRepository = new LeaseRepository();
        }

        // GET: Leases
        public ActionResult Index()
        {
            var leases = _leaseRepository.GetAll();
            return View(leases);
        }

        // GET: Leases/Details/5
        public ActionResult Details(int id)
        {
            var lease = _leaseRepository.GetById(id);

            if (lease == null)
            {
                return HttpNotFound();
            }

            return View(lease);
        }

        // GET: Leases/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Leases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LeaseAgreement lease)
        {
            if (ModelState.IsValid)
            {
                _leaseRepository.Add(lease);
                return RedirectToAction("Index");
            }

            return View(lease);
        }

        // GET: Leases/Edit/5
        public ActionResult Edit(int id)
        {
            var lease = _leaseRepository.GetById(id);

            if (lease == null)
            {
                return HttpNotFound();
            }

            return View(lease);
        }

        // POST: Leases/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LeaseAgreement lease)
        {
            if (ModelState.IsValid)
            {
                _leaseRepository.Update(lease);
                return RedirectToAction("Index");
            }

            return View(lease);
        }

        // GET: Leases/Delete/5
        public ActionResult Delete(int id)
        {
            var lease = _leaseRepository.GetById(id);

            if (lease == null)
            {
                return HttpNotFound();
            }

            return View(lease);
        }

        // POST: Leases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _leaseRepository.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
