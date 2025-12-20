using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace FitHub.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            if (string.Equals(controller, "Uye", StringComparison.OrdinalIgnoreCase)
                && (string.Equals(action, "Login", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(action, "Register", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            var role = context.HttpContext.Session.GetString("UserRole") ?? string.Empty;

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("Login", "Uye", null);
            }
        }
    }
}
