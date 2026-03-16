#region

using System.Collections.Generic;
using System.Web.SessionState;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.RequestLogger;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Infrastructure.Contexts;
using R2V2.Web.Infrastructure.MvcFramework.Filters;

#endregion

namespace R2V2.Web.Infrastructure
{
    public class SessionCleanupService
    {
        private static readonly ILog<SessionCleanupService> Log = new Log<SessionCleanupService>();

        public static void Clean(HttpSessionState session)
        {
            // application session 
            var applicationSession = (ApplicationSession)session[RequestLoggerFilter.ApplicationSessionKey];
            if (applicationSession != null)
            {
                Log.Info(applicationSession.ToDebugString());
            }

            // authenticated institution
            var authenticatedInstitution =
                (AuthenticatedInstitution)session[AuthenticationContext.AuthenticatedInstitutionKey];
            if (authenticatedInstitution != null)
            {
                Log.Info(authenticatedInstitution.ToDebugString());
                authenticatedInstitution.ClearLicenses();
            }

            // PDA resource ids
            var pdaResourceIds = (List<int>)session[PatronDrivenAcquisitionService.PatronDrivenAcquisitionResourceKey];
            if (pdaResourceIds != null)
            {
                Log.InfoFormat("pdaResourceIds.Count: {0}", pdaResourceIds.Count);
                pdaResourceIds.Clear();
            }

            // current admin institution
            var adminInstitution = (IAdminInstitution)session[AdminContext.AdminInstitutionKey];
            if (adminInstitution != null)
            {
                Log.Info(adminInstitution.ToDebugString());
                adminInstitution.ClearLicenses();
            }

            //Active.Cart
            var cart = (CachedCart)session[CartService.ActiveCartKey];
            if (cart != null)
            {
                Log.Info(cart.ToDebugString());
                cart.ClearItems();
            }
        }
    }
}