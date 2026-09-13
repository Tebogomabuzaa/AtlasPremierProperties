using System;
using System.Web.Mvc;
using AtlasPremierProperties.Helpers;
using AtlasPremierProperties.Models.Entities;
using AtlasPremierProperties.Repositories;

namespace AtlasPremierProperties.Controllers
{
    public class OwnersController : Controller
    {
        private readonly OwnersRepository _ownerRepo;

        public OwnersController()
        {
            _ownerRepo = new OwnersRepository();
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
        public ActionResult Create([Bind(Exclude = "PasswordHash")] Owners owners)
        {
            if (!ModelState.IsValid)
            {
                return View(owners);
            }

            try
            {
                SetPortalPassword(owners);
                int newId = _ownerRepo.Add(owners);
                TempData["Success"] =
                    "Owner successfully added. Owner ID: " + newId;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(owners);
            }
        }

        // Opens the edit form with the owner's current details filled in
        public ActionResult Edit(int id)
        {
            var owners = _ownerRepo.GetById(id);
            if (owners == null) return HttpNotFound();

            return View(owners);
        }

        // Saves the updated owner details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Exclude = "PasswordHash")] Owners owners)
        {
            if (!ModelState.IsValid) return View(owners);

            try
            {
                SetPortalPassword(owners);
                _ownerRepo.Update(owners);
                TempData["Success"] =
                    "Owner ID " + owners.OwnerID + " successfully updated.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(owners);
            }
        }

        // Confirmation page before deleting
        public ActionResult Delete(int id)
        {
            var owners = _ownerRepo.GetById(id);
            if (owners == null) return HttpNotFound();

            return View(owners);
        }

        // Handles the delete request after confirmation
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

        private static void SetPortalPassword(Owners owners)
        {
            if (!string.IsNullOrEmpty(owners.PortalPassword))
            {
                owners.PasswordHash = PasswordHasher.Hash(owners.PortalPassword);
            }
        }
    }
}
