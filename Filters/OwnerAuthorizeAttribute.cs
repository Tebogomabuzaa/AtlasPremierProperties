using System.Web.Mvc;
using System.Web.Routing;
using AtlasPremierProperties.Helpers;

namespace AtlasPremierProperties.Filters
{
    public class OwnerAuthorizeAttribute : AuthorizeAttribute
    {
        public OwnerAuthorizeAttribute()
        {
            Roles = AuthCookie.OwnerRole;
        }

        // Forms authentication only knows the staff login URL, so send owners to their own sign-in page.
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
            {
                { "controller", "Account" },
                { "action", "OwnerLogin" },
                { "returnUrl", filterContext.HttpContext.Request.RawUrl }
            });
        }
    }
}
