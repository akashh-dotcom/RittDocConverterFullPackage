#region

using System.Web.Mvc;
using log4net;
using R2V2.Extensions;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Exceptions;
using R2V2.Web.Infrastructure.Contexts;
using R2V2.Web.Models.Error;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public class DefaultExceptionFilter : IR2V2Filter, IExceptionFilter
    {
        private readonly IExceptionResultFilter _exceptionResultFilter;
        private readonly ILog<DefaultExceptionFilter> _log;

        public DefaultExceptionFilter(
            ILog<DefaultExceptionFilter> log,
            IExceptionResultFilter exceptionResultFilter)
        {
            _log = log;
            _exceptionResultFilter = exceptionResultFilter;
        }

        public void OnException(ExceptionContext context)
        {
            if (SystemInformation.IsInDebugMode)
            {
                return;
            }

            context.HttpContext.Response.TrySkipIisCustomErrors = true;
            string errorId;
            if (context.Exception is IExpectedException)
            {
                var exceptedException = context.Exception.As<IExpectedException>();

                context.ExceptionHandled = true;
                _log.Warn(context.Exception.Message, context.Exception);
                errorId = LogicalThreadContext.Properties["requestId"].ToString();

                if (context.HttpContext.Request.Url != null)
                {
                    context.Controller.ViewData.Model = new ExpectedErrorViewModel
                    {
                        ErrorId = errorId,
                        AspxErrorPath = context.HttpContext.Request.Url.ToString(),
                        ErrorMessage = exceptedException.UserFriendlyMessage
                    };
                }

                context.Result = new ViewResult
                {
                    ViewData = context.Controller.ViewData,
                    TempData = context.Controller.TempData,
                    ViewName = "~/Views/Error/ExpectedError.cshtml"
                };


                context.ExceptionHandled = true;
                context.HttpContext.Response.Clear();
                context.HttpContext.Response.StatusCode = exceptedException.HttpStatusCode;
                context.HttpContext.Response.SubStatusCode = exceptedException.HttpSubStatusCode;
                context.HttpContext.Response.AddHeader(SystemInformation.ApplicationName.Append(".ErrorId"), errorId);

                _exceptionResultFilter.Execute(context);
                return;
            }

            _log.Fatal(context.Exception.Message, context.Exception);
            errorId = LogicalThreadContext.Properties["requestId"].ToString();

            if (context.HttpContext.Request.Url != null)
            {
                context.Controller.ViewData.Model = new InternalServerViewModel
                {
                    ErrorId = errorId,
                    AspxErrorPath = context.HttpContext.Request.Url.ToString()
                };
            }

            context.Result = new ViewResult
            {
                ViewData = context.Controller.ViewData,
                TempData = context.Controller.TempData,
                ViewName = "~/Views/Error/InternalServer.cshtml"
            };

            context.ExceptionHandled = true;
            context.HttpContext.Response.Clear();
            context.HttpContext.Response.StatusCode = 500;
            context.HttpContext.Response.AddHeader(SystemInformation.ApplicationName.Append(".ErrorId"), errorId);

            _exceptionResultFilter.Execute(context);
        }

        public bool AllowMultiple => true;

        public int Order => 1;

        public FilterScope FilterScope => FilterScope.Global;

        public bool CanProcess(ActionContext actionContext)
        {
            return true;
        }
    }
}