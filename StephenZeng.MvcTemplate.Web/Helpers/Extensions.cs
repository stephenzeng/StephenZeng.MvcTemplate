using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace StephenZeng.MvcTemplate.Web.Helpers
{
    public static class Extensions
    {
        public static string GetClaimValue(this IIdentity identity, string claimType)
        {
            var claimsIdentity = identity as ClaimsIdentity;
            if (claimsIdentity == null)
                return null;

            var claim = claimsIdentity.Claims.FirstOrDefault(c => c.Type == claimType);

            return claim == null ? null : claim.Value;
        }

        public static MvcHtmlString GoBackTo(this UrlHelper urlHelper,
            string text,
            string action,
            string controller = null)
        {
            var url = urlHelper.Action(action, controller);

            var output = string.Format(@"<a href=""{0}""><i class=""fa fa-arrow-circle-left""></i> {1}</a>",
                url, text);

            return new MvcHtmlString(output);
        }

        public static MvcHtmlString ToIcon(this bool input)
        {
            var className = input ? "fa-check" : "fa-remove";
            return new MvcHtmlString(string.Format(@"<span class=""fa {0}""></span>", className));
        }

        public static IList<string> SplitToList(this string input)
        {
            if (string.IsNullOrEmpty(input)) return Enumerable.Empty<string>().ToList();

            return input.Split(',').ToList();
        }

        public static string ActivateCurrentMenu(this HtmlHelper htmlHelper, string controller)
        {
            var currentController = htmlHelper.ViewContext.RouteData.Values["controller"].ToString();
            return controller == currentController ? "active" : string.Empty;
        }

        public static string GetIp(this HttpContextBase context)
        {
            if (context == null || context.Request == null)
                return string.Empty;

            return context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? context.Request.UserHostAddress;
        }

        public static string GetDisplayName<T>(this T value)
        {
            if (value == null) return string.Empty;

            var field = value.GetType().GetField(value.ToString());

            if (field == null) return value.ToString();

            var list = field.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];

            if (list == null || !list.Any())
                return value.ToString();

            return list.First().Name;
        }
    }
}