#region

using System.Security.Principal;
using System.Web.Security;

#endregion

namespace R2V2.Web.Infrastructure.Authentication
{
    public interface IFormsAuthenticationService
    {
        void SetTicket(IPrincipal principal);
        void RemoveTicket();
    }

    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        public void SetTicket(IPrincipal principal)
        {
            FormsAuthentication.SetAuthCookie(principal.Identity.Name, false);
        }

        public void RemoveTicket()
        {
            FormsAuthentication.SignOut();
        }
    }
}