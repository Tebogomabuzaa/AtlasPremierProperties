using System;
using System.Web.Mvc;
using AtlasPremierProperties.Models.Entities;
using AtlasPremierProperties.Repositories;

namespace AtlasPremierProperties.Controllers
{
    public class OwnersController : Controller
    {

        private readonly OwnerRepository _ownerRepo;


        public OwnersController()
        {
            _ownerRepo = new OwnerRepository();
        }

        // Lists all owners
        public ActionResult Index()
        {
            return View(_ownerRepo.GetAll());
        }

        // Empty Create form
        public ActionResult Create()
        {
            return View();
        }

        // Handles the submitted Create form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Owner owner)
        {
            if (!ModelState.IsValid)
            {
                return View(owner);
            }

            try
            {
                int newId = _ownerRepo.Add(owner);
                TempData["Success"] =
                    "Owner successfully added. Owner ID: " + newId;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                
                ModelState.AddModelError("", ex.Message);
                return View(owner);
            }
        }

        // Opens the edit form with the owner's current details filled in
        public ActionResult Edit(int id)
        {
            var owner = _ownerRepo.GetById(id);
            if (owner == null) return HttpNotFound();

            return View(owner);
        }

        // Saves the updated owner details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Owner owner)
        {
            if (!ModelState.IsValid) return View(owner);

            _ownerRepo.Update(owner);
            TempData["Success"] =
                "Owner ID " + owner.OwnerID + " successfully updated.";
            return RedirectToAction("Index");
        }

        //Confirmation page before deleting
        public ActionResult Delete(int id)
        {
            var owner = _ownerRepo.GetById(id);
            if (owner == null) return HttpNotFound();

            return View(owner);
        }

        // Handle sthe delete request after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                _ownerRepo.Delete(id);
                TempData["Success"] = "Owner ID " + id + " successfully deleted.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}