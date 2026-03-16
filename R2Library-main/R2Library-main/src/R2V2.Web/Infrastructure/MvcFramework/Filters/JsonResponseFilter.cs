#region

using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public class JsonResponseFilter : R2V2ResultFilter
    {
        private readonly IAuthenticationContext _authenticationContext;

        public JsonResponseFilter(IAuthenticationContext authenticationContext)
            : base(authenticationContext)
        {
            _authenticationContext = authenticationContext;
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var jsonResult = filterContext.Result as JsonResult;
            if (jsonResult == null)
            {
                return;
            }

            var jsonResponse = jsonResult.Data as JsonResponse;
            if (jsonResponse == null)
            {
                return;
            }

            jsonResponse.Timeout = !_authenticationContext.IsAuthenticated;
        }
    }
}