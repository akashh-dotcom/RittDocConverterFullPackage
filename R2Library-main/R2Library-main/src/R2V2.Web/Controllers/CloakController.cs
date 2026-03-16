#region

using System.Text;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Helpers;
using R2V2.Web.Models.Cloak;

#endregion

namespace R2V2.Web.Controllers
{
    public class CloakController : R2BaseController
    {
        private readonly ILog<ResourceController> _log;

        public CloakController(ILog<ResourceController> log, IAuthenticationContext authenticationContext)
            : base(authenticationContext)
        {
            _log = log;
        }

        public ActionResult Index(string requestId)
        {
            var message = new StringBuilder()
                .Append("/Cloak/Index has been accessed!").AppendLine();

            if (AuthenticatedInstitution != null)
            {
                message.AppendFormat("\tInstitution: {0}, {1}", AuthenticatedInstitution.AccountNumber,
                    AuthenticatedInstitution.Name).AppendLine();
                if (AuthenticatedInstitution.User != null)
                {
                    message.AppendFormat("\tUser: {0}, {1}", AuthenticatedInstitution.User.Id,
                            AuthenticatedInstitution.User.ToFullName())
                        .AppendLine();
                }
                else
                {
                    message.AppendLine("\tUser: null");
                }
            }
            else
            {
                message.AppendLine("\tInstitution: null");
            }

            message.AppendFormat("\tIP: {0}", Request.GetHostIpAddress()).AppendLine();
            message.AppendFormat("\tSessionID: {0}", Session.SessionID).AppendLine();
            message.AppendFormat("\tUserAgent: {0}", Request.UserAgent).AppendLine();
            _log.WarnFormat(message.ToString());

            var model = new CloakIndex();
            return View(model);
        }

        public ActionResult Dagger(string requestId)
        {
            var message = new StringBuilder()
                .Append("/Cloak/Dagger has been accessed!").AppendLine();

            if (AuthenticatedInstitution != null)
            {
                message.AppendFormat("\tInstitution: {0}, {1}", AuthenticatedInstitution.AccountNumber,
                    AuthenticatedInstitution.Name).AppendLine();
                if (AuthenticatedInstitution.User != null)
                {
                    message.AppendFormat("\tUser: {0}, {1}", AuthenticatedInstitution.User.Id,
                            AuthenticatedInstitution.User.ToFullName())
                        .AppendLine();
                }
                else
                {
                    message.AppendLine("\tUser: null");
                }
            }
            else
            {
                message.AppendLine("\tInstitution: null");
            }

            message.AppendFormat("\tIP: {0} - [BLOCKED]", Request.GetHostIpAddress()).AppendLine();
            message.AppendFormat("\tSessionID: {0}", Session.SessionID).AppendLine();
            message.AppendFormat("\tUserAgent: {0}", Request.UserAgent).AppendLine();
            _log.WarnFormat(message.ToString());

            var model = new CloakIndex();
            return View(model);
        }
    }
}