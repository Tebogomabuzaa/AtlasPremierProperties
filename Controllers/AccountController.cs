using System.Web.Mvc;
using System.Web.Security;
using AtlasPremierProperties.Helpers;
using AtlasPremierProperties.Models.ViewModels;
using AtlasPremierProperties.Repositories;

namespace AtlasPremierProperties.Controllers
{
    public class AccountController : Controller
    {
        private readonly SystemUserRepository _users = new SystemUserRepository();
        private readonly OwnersRepository _owners = new OwnersRepository();

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.IsInRole(AuthCookie.StaffRole)) return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.NeedsSetup = !_users.Any();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var user = _users.GetByUsername(model.Username);
            if (user == null || !PasswordHasher.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            AuthCookie.SignIn(Response, user.Username, AuthCookie.StaffRole, user.UserID);
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult OwnerLogin(string returnUrl)
        {
            if (User.IsInRole(AuthCookie.OwnerRole)) return RedirectToAction("Index", "OwnerPortal");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult OwnerLogin(OwnerLoginViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var owner = _owners.GetByEmail(model.Email);
            if (owner == null || !PasswordHasher.Verify(model.Password, owner.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            AuthCookie.SignIn(Response, owner.FullName, AuthCookie.OwnerRole, owner.OwnerID);
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index", "OwnerPortal");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Landing");
        }

        [AllowAnonymous]
        public ActionResult Setup()
        {
            if (!CanRunSetup()) return HttpNotFound();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Setup(SetupViewModel model)
        {
            if (!CanRunSetup()) return HttpNotFound();
            if (!ModelState.IsValid) return View(model);

            int userId = _users.Add(model.Username, PasswordHasher.Hash(model.Password), "Administrator");
            AuthCookie.SignIn(Response, model.Username, AuthCookie.StaffRole, userId);
            return RedirectToAction("Index", "Home");
        }

        // The first administrator can only be created from the server itself, before any user exists.
        private bool CanRunSetup()
        {
            return Request.IsLocal && !_users.Any();
        }
    }
}
