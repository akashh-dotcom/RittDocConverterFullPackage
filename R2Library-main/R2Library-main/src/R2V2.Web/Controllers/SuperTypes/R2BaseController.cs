#region

using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Web.Optimization;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.RequestLogger;
using R2V2.Infrastructure.Authentication;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.MvcFramework.Filters;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Controllers.SuperTypes
{
    [DomainVerifyFilter]
    [RequestLoggerFilter]
    [RequireSslFilter]
    public abstract class R2BaseController : Controller, IR2BaseController
    {
        protected readonly IAuthenticationContext AuthenticationContext;

        protected R2BaseController()
        {
        }

        protected R2BaseController(IAuthenticationContext authenticationContext)
        {
            AuthenticationContext = authenticationContext;
        }

        protected AuthenticatedInstitution AuthenticatedInstitution => AuthenticationContext.AuthenticatedInstitution;

        protected IUser CurrentUser
        {
            get
            {
                if (AuthenticationContext.IsAuthenticated && AuthenticatedInstitution != null &&
                    AuthenticatedInstitution.User != null)
                {
                    return AuthenticatedInstitution.User;
                }

                return null;
            }
        }

        protected int UserId
        {
            get
            {
                var currentUser = CurrentUser;
                return currentUser != null ? currentUser.Id : 0;
            }
        }

        /// <summary>
        ///     Render partial view to a string for use with JSON
        ///     http://craftycodeblog.com/2010/05/15/asp-net-mvc-render-partial-view-to-string/
        /// </summary>
        protected string RenderPartialViewToString()
        {
            return RenderPartialViewToString(null, null);
        }

        /// <summary>
        ///     Render partial view to a string for use with JSON
        ///     http://craftycodeblog.com/2010/05/15/asp-net-mvc-render-partial-view-to-string/
        /// </summary>
        protected string RenderPartialViewToString(string viewName)
        {
            return RenderPartialViewToString(viewName, null);
        }

        /// <summary>
        ///     Render partial view to a string for use with JSON
        ///     http://craftycodeblog.com/2010/05/15/asp-net-mvc-render-partial-view-to-string/
        /// </summary>
        protected string RenderPartialViewToString(object model)
        {
            return RenderPartialViewToString(null, model);
        }

        /// <summary>
        ///     Render partial view to a string for use with JSON
        ///     http://craftycodeblog.com/2010/05/15/asp-net-mvc-render-partial-view-to-string/
        /// </summary>
        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = ControllerContext.RouteData.GetRequiredString("action");
            }

            ViewData.Model = model;

            string html;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                html = sw.GetStringBuilder().ToString();
            }

            return html.Trim();
        }

        public string RenderRazorViewToString(string controllerName, string viewName, object model)
        {
            var baseModel = model as IR2V2Model;
            if (baseModel != null)
            {
                baseModel.Footer = new Footer();
            }

            var cssFiles = @BundleResolver.Current.GetBundleContents("~/_Static/Css/r2.email.css");

            var sb = new StringBuilder();
            foreach (var cssFile in cssFiles)
            {
                sb.Append(System.IO.File.ReadAllText(Server.MapPath(cssFile)));
            }

            ViewData.Model = model;

            var view = $"~/Views/{controllerName}/{viewName}.cshtml";

            using (var sw = new StringWriter())
            {
                var viewResult =
                    ViewEngines.Engines.FindView(ControllerContext, view, "~/Views/Shared/_Layout.Email.cshtml");

                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);

                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                var messageBody = sw.GetStringBuilder().ToString().Replace("[[CSS]]", sb.ToString());
                return messageBody;
            }
        }

        protected void SuppressRequestLogging()
        {
            var requestData = HttpContext.RequestStorage().Get<RequestData>(RequestLoggerFilter.RequestDataKey);
            if (requestData != null)
            {
                requestData.DoNotLogRequest();
            }
        }

        protected bool IsCaptchaValid(bool captchaValid)
        {
            if (captchaValid)
            {
                if (Session["BDC_IsCaptchaSolved_Captcha"] != null)
                {
                    Session["BDC_IsCaptchaSolved_Captcha"] = false;
                }

                return true;
            }

            return false;
        }
    }
}