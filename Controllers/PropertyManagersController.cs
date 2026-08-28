using System;
using System.Web.Mvc;
using AtlasPremierProperties.Models.Entities;
using AtlasPremierProperties.Repositories;

namespace AtlasPremierProperties.Controllers
{
    public class PropertyManagersController : Controller
    {
        private readonly IPropertyManagerRepository _repository;

        public PropertyManagersController() : this(new PropertyManagerRepository()) { }

        public PropertyManagersController(IPropertyManagerRepository repository)
        {
            _repository = repository;
        }

        // GET: PropertyManagers
        public ActionResult Index()
        {
            var managers = _repository.GetAll();
            return View(managers);
        }

        // GET: PropertyManagers/Create
        public ActionResult Create()
        {
            return View(new PropertyManager { DateHired = DateTime.Today });
        }

        // POST: PropertyManagers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PropertyManager manager)
        {
            if (!ModelState.IsValid)
            {
                return View(manager);
            }

            if (_repository.EmailExists(manager.Email))
            {
                ModelState.AddModelError("Email", "A property manager with this email already exists.");
                return View(manager);
            }

            int newId = _repository.Add(manager);
            TempData["SuccessMessage"] =
                $"Property Manager successfully added (Manager ID: {newId}).";
            return RedirectToAction("Index");
        }

        // GET: PropertyManagers/Edit/5
        public ActionResult Edit(int id)
        {
            var manager = _repository.GetById(id);
            if (manager == null)
            {
                return HttpNotFound();
            }
            return View(manager);
        }

        // POST: PropertyManagers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PropertyManager manager)
        {
            if (!ModelState.IsValid)
            {
                return View(manager);
            }

            if (_repository.EmailExists(manager.Email, manager.ManagerID))
            {
                ModelState.AddModelError("Email", "Another property manager already uses this email.");
                return View(manager);
            }

            _repository.Update(manager);
            TempData["SuccessMessage"] =
                $"Property Manager (ID: {manager.ManagerID}) successfully updated.";
            return RedirectToAction("Index");
        }

        // GET: PropertyManagers/Delete/5
        public ActionResult Delete(int id)
        {
            var manager = _repository.GetById(id);
            if (manager == null)
            {
                return HttpNotFound();
            }
            return View(manager);
        }

        // POST: PropertyManagers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _repository.Delete(id);
            TempData["SuccessMessage"] =
                $"Property Manager (ID: {id}) successfully deleted.";
            return RedirectToAction("Index");
        }
    }
}
