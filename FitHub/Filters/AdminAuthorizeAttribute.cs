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

            // Allow Uye/Login and Uye/Register for everyone
            if (controller == "Uye" &&
                (action == "Login" || action == "Register"))
            {
                return;
            }

            var role = context.HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                context.Result = new RedirectToActionResult("Login", "Uye", null);
            }
        }
    }
}
