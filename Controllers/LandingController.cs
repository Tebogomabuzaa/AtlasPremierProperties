using System.Web.Mvc;
using AtlasPremierProperties.Helpers;

namespace AtlasPremierProperties.Controllers
{
    [AllowAnonymous]
    public class LandingController : Controller
    {
        public ActionResult Index()
        {
            if (User.IsInRole(AuthCookie.StaffRole)) return RedirectToAction("Index", "Home");
            if (User.IsInRole(AuthCookie.OwnerRole)) return RedirectToAction("Index", "OwnerPortal");
            return View();
        }
    }
}
