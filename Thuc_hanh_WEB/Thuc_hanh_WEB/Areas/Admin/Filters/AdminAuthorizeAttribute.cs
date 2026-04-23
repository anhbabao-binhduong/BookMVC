using System.Web.Mvc;
using System.Web.Routing;

namespace Thuc_hanh_WEB.Filters
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            var role = httpContext.Session["AdminRole"];
            return role != null && role.ToString() == "Admin";
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary {
                    { "area", "Admin" },
                    { "controller", "Auth" },
                    { "action", "Login" }
                });
        }
    }
}