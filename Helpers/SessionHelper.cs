using Microsoft.AspNetCore.Http;

namespace Contoso.Helpers
{
    public static class SessionHelper
    {
        public static bool IsUserLoggedIn(HttpContext httpContext)
        {
            var userID = httpContext.Session.GetString("UserID");
            return !string.IsNullOrEmpty(userID);
        }

        public static string? GetUserRole(HttpContext httpContext)
        {
            return httpContext.Session.GetString("UserRole");
        }

        public static int? GetUserId(HttpContext httpContext)
        {
            var userID = httpContext.Session.GetString("UserID");
            return int.TryParse(userID, out int id) ? id : null;
        }
    }
}