using System.Data.OleDb;
using System.Web.Mvc;
using AtlasPremierProperties.Models.Entities;
using AtlasPremierProperties.Repositories;

namespace AtlasPremierProperties.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly PropertyRepository _properties = new PropertyRepository();
        private readonly OwnersRepository _owners = new OwnersRepository();

        public ActionResult Index()
        {
            return View(_properties.GetAll());
        }

        public ActionResult Create()
        {
            PopulateOwners(null);
            return View(new Property { VacancyStatus = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Property property)
        {
            if (ModelState.IsValid && _properties.AddressExists(property.PhysicalAddress))
            {
                ModelState.AddModelError("PhysicalAddress", "A property with this address already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int newId = _properties.Add(property);
                    TempData["Success"] = "Property successfully added. Property ID: " + newId;
                    return RedirectToAction("Index");
                }
                catch (OleDbException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            PopulateOwners(property.OwnerID);
            return View(property);
        }

        public ActionResult Edit(int id)
        {
            var property = _properties.GetById(id);
            if (property == null) return HttpNotFound();

            PopulateOwners(property.OwnerID);
            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Property property)
        {
            if (ModelState.IsValid && _properties.AddressExists(property.PhysicalAddress, property.PropertyID))
            {
                ModelState.AddModelError("PhysicalAddress", "Another property already uses this address.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _properties.Update(property);
                    TempData["Success"] = "Property ID " + property.PropertyID + " successfully updated.";
                    return RedirectToAction("Index");
                }
                catch (OleDbException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            PopulateOwners(property.OwnerID);
            return View(property);
        }

        public ActionResult Delete(int id)
        {
            var property = _properties.GetById(id);
            if (property == null) return HttpNotFound();

            return View(property);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (_properties.HasLeases(id))
            {
                TempData["Error"] = "Property ID " + id + " has lease agreements linked and cannot be deleted.";
                return RedirectToAction("Index");
            }

            _properties.Delete(id);
            TempData["Success"] = "Property ID " + id + " successfully deleted.";
            return RedirectToAction("Index");
        }

        private void PopulateOwners(int? selectedId)
        {
            ViewBag.Owners = new SelectList(_owners.GetAll(), "OwnerID", "FullName", selectedId);
        }
    }
}
