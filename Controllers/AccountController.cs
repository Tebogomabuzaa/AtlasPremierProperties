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

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (Request.IsAuthenticated) return RedirectToAction("Index", "Home");

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

            FormsAuthentication.SetAuthCookie(user.Username, false);
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
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

            _users.Add(model.Username, PasswordHasher.Hash(model.Password), "Administrator");
            FormsAuthentication.SetAuthCookie(model.Username, false);
            return RedirectToAction("Index", "Home");
        }

        // The first administrator can only be created from the server itself, before any user exists.
        private bool CanRunSetup()
        {
            return Request.IsLocal && !_users.Any();
        }
    }
}
