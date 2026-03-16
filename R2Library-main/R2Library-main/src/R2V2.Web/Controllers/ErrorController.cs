#region

using System.Web.Mvc;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Models.Error;

#endregion

namespace R2V2.Web.Controllers
{
    public class ErrorController : R2BaseController
    {
        private const string ErrorIdKey = "R2V2.ErrorId";
        //
        // GET: /Error/

        public ActionResult NotFound(string aspxerrorpath)
        {
            var errorId = "";
            if (HttpContext.Items[ErrorIdKey] != null)
            {
                errorId = HttpContext.Items[ErrorIdKey].ToString();
                HttpContext.Response.AddHeader("ErrorId", errorId);
            }

            HttpContext.Response.StatusCode = 404;

            var urlPath = string.IsNullOrEmpty(aspxerrorpath) ? HttpContext.Request.RawUrl : aspxerrorpath;

            return View(new NotFoundViewModel { ErrorId = errorId, AspxErrorPath = urlPath });
        }

        public ActionResult InternalServer(string aspxerrorpath)
        {
            var errorId = "";

            if (HttpContext.Items[ErrorIdKey] != null)
            {
                errorId = HttpContext.Items[ErrorIdKey].ToString();
                HttpContext.Response.AddHeader("ErrorId", errorId);
            }

            HttpContext.Response.StatusCode = 500;

            var urlPath = string.IsNullOrEmpty(aspxerrorpath) ? HttpContext.Request.RawUrl : aspxerrorpath;

            return View(new InternalServerViewModel { ErrorId = errorId, AspxErrorPath = urlPath });
        }
    }
}