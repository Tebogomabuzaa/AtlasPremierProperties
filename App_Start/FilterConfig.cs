using System.Web.Mvc;
using AtlasPremierProperties.Helpers;

namespace AtlasPremierProperties
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthorizeAttribute { Roles = AuthCookie.StaffRole });
        }
    }
}
