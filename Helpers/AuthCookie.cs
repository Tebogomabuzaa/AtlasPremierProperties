using System;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace AtlasPremierProperties.Helpers
{
    public static class AuthCookie
    {
        public const string StaffRole = "Staff";
        public const string OwnerRole = "Owner";

        // UserData holds "role|id" so an owner's cookie also says which owner's properties they may see.
        public static void SignIn(HttpResponseBase response, string name, string role, int subjectId)
        {
            var ticket = new FormsAuthenticationTicket(
                1,
                name,
                DateTime.Now,
                DateTime.Now.Add(FormsAuthentication.Timeout),
                false,
                role + "|" + subjectId,
                FormsAuthentication.FormsCookiePath);

            response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Path = FormsAuthentication.FormsCookiePath
            });
        }

        public static void AttachRoles(HttpContext context)
        {
            var identity = context.User?.Identity as FormsIdentity;
            if (identity == null || !identity.IsAuthenticated) return;

            string role = Parse(identity.Ticket.UserData, out _);
            context.User = new GenericPrincipal(identity, role == null ? new string[0] : new[] { role });
        }

        public static int SubjectId(IPrincipal user)
        {
            var identity = user?.Identity as FormsIdentity;
            if (identity == null) return 0;

            Parse(identity.Ticket.UserData, out int id);
            return id;
        }

        private static string Parse(string userData, out int id)
        {
            id = 0;
            var parts = (userData ?? string.Empty).Split('|');
            if (parts.Length != 2 || !int.TryParse(parts[1], out id)) return null;
            return parts[0];
        }
    }
}
