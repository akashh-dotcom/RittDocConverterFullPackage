#region

using System.Linq;
using System.Net;
using System.Web.Mvc;
using R2V2.Core.Institution;
using R2V2.Extensions;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.RequestLogger;

#endregion

namespace R2V2.Web.Controllers
{
    public class TrustedAuthenticationController : Controller
    {
        private readonly ILog<TrustedAuthenticationController> _log;
        private readonly TrustedAuthenticationService _trustedAuthenicationService;
        private readonly WebServiceAuthenticationFactory _webServiceAuthenticationFactory;

        public TrustedAuthenticationController(ILog<TrustedAuthenticationController> log,
            WebServiceAuthenticationFactory webServiceAuthenticationFactory,
            TrustedAuthenticationService trustedAuthenicationService)
        {
            _log = log;
            _webServiceAuthenticationFactory = webServiceAuthenticationFactory;
            _trustedAuthenicationService = trustedAuthenicationService;
        }

        //
        // GET: /WebServiceAuthentication/
        [IgnoreRequest]
        public ActionResult Index(string authenticationKey)
        {
            var strIpAddress = Request.GetHostIpAddress();
            if (string.IsNullOrWhiteSpace(strIpAddress))
            {
                return Json(new WebTrustedAuthentication { ErrorMessage = "IP address is null or empty" },
                    JsonRequestBehavior.AllowGet);
            }

            _log.DebugFormat("REMOTE_ADDR - ip: {0}", strIpAddress);


            var ipAddress = IPAddress.Parse(strIpAddress);
            var clientIpNumber = ipAddress.ToIpNumber();
            _log.DebugFormat("WebServiceAuthenticationController Index() - ip: {0}, ip number: {1}", strIpAddress,
                clientIpNumber);

            WebTrustedAuthentication webTrustedAuthentication;

            //webServiceAuthentication.AuthenticationKey == authenticationKey
            var test = _webServiceAuthenticationFactory.GetWebServiceAuthentications(clientIpNumber);
            if (test.Any())
            {
                var webServiceAuthentication2 = test.FirstOrDefault(x => x.AuthenticationKey == authenticationKey);
                if (webServiceAuthentication2 != null)
                {
                    webTrustedAuthentication =
                        _trustedAuthenicationService.GetWebTrustedAuthentication(webServiceAuthentication2,
                            authenticationKey);
                }
                else
                {
                    webTrustedAuthentication = new WebTrustedAuthentication
                    {
                        ErrorMessage =
                            "Your IP address could not be found in our system. Please use the contact us link on R2library.com to resolve this issue."
                    };
                }
            }
            else
            {
                webTrustedAuthentication = new WebTrustedAuthentication
                {
                    ErrorMessage =
                        "Your IP address could not be found in our system. Please use the contact us link on R2library.com to resolve this issue."
                };
            }

            ////return Content("The IP address and the authentication key provided cannot be found. Please use the contact us link on R2library.com to resolve this issue.");
            return Json(webTrustedAuthentication, JsonRequestBehavior.AllowGet);
        }
    }
}