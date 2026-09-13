using System.Web.Mvc;
using System.Web.Security;
using AtlasPremierProperties.Filters;
using AtlasPremierProperties.Helpers;
using AtlasPremierProperties.Models.ViewModels;
using AtlasPremierProperties.Repositories;

namespace AtlasPremierProperties.Controllers
{
    [OverrideAuthorization]
    [OwnerAuthorize]
    public class OwnerPortalController : Controller
    {
        private readonly OwnersRepository _owners = new OwnersRepository();
        private readonly PropertyRepository _properties = new PropertyRepository();

        public ActionResult Index()
        {
            var owner = _owners.GetById(AuthCookie.SubjectId(User));
            if (owner == null)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("OwnerLogin", "Account");
            }

            return View(new OwnerPortalViewModel
            {
                Owner = owner,
                Properties = _properties.GetByOwner(owner.OwnerID)
            });
        }
    }
}
