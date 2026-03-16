#region

using System;
using R2V2.Contexts;
using R2V2.Core.Admin;
using R2V2.Core.Institution;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Storages;

#endregion

namespace R2V2.Web.Infrastructure.Contexts
{
    public class AdminContext : IAdminContext
    {
        public const string AdminInstitutionKey = "Current.AdminInstitution";
        private readonly Func<IAuthenticationContext> _authenticationContextFactory;
        private readonly Func<IAuthenticationService> _authenticationServiceFactory;
        private readonly Func<InstitutionService> _institutionServiceFactory;

        private readonly Func<IUserSessionStorageService> _userSessionStorageServiceFactory;

        public AdminContext(Func<IUserSessionStorageService> userSessionStorageServiceFactory
            , Func<InstitutionService> institutionServiceFactory
            , Func<AuthenticationContext> authenticationContextFactory
            , Func<IAuthenticationService> authenticationServiceFactory
        )
        {
            _userSessionStorageServiceFactory = userSessionStorageServiceFactory;
            _institutionServiceFactory = institutionServiceFactory;
            _authenticationContextFactory = authenticationContextFactory;
            _authenticationServiceFactory = authenticationServiceFactory;
        }

        private IUserSessionStorageService UserSessionStorageService => _userSessionStorageServiceFactory();

        private InstitutionService InstitutionService => _institutionServiceFactory();

        private IAuthenticationContext AuthenticationContext => _authenticationContextFactory();

        public IAdminInstitution GetAdminInstitution(int institutionId)
        {
            var authenticatedInstitution = AuthenticationContext.AuthenticatedInstitution;
            IAdminInstitution adminInstitution = UserSessionStorageService.Get<AdminInstitution>(AdminInstitutionKey);

            if (adminInstitution == null || adminInstitution.Id == 0)
            {
                if (authenticatedInstitution.Id == institutionId)
                {
                    adminInstitution = new AdminInstitution(authenticatedInstitution);
                }
                else if (adminInstitution == null && authenticatedInstitution != null &&
                         !(authenticatedInstitution.IsRittenhouseAdmin() ||
                           authenticatedInstitution.IsSalesAssociate()))
                {
                    adminInstitution =
                        new AdminInstitution(InstitutionService.GetInstitutionForAdmin(authenticatedInstitution.Id));
                }
                else if (authenticatedInstitution.IsRittenhouseAdmin() || authenticatedInstitution.IsSalesAssociate())
                {
                    adminInstitution = new AdminInstitution(InstitutionService.GetInstitutionForAdmin(institutionId));
                }

                UserSessionStorageService.Put(AdminInstitutionKey, adminInstitution);
            }
            else
            {
                if (adminInstitution.Id != institutionId)
                {
                    if (authenticatedInstitution.IsRittenhouseAdmin() || authenticatedInstitution.IsSalesAssociate())
                    {
                        adminInstitution = institutionId == 0
                            ? new AdminInstitution(
                                InstitutionService.GetInstitutionForAdmin(authenticatedInstitution.Id))
                            : new AdminInstitution(InstitutionService.GetInstitutionForAdmin(institutionId));

                        UserSessionStorageService.Put(AdminInstitutionKey, adminInstitution);
                    }
                    else if (authenticatedInstitution != null)
                    {
                        adminInstitution =
                            new AdminInstitution(
                                InstitutionService.GetInstitutionForAdmin(authenticatedInstitution.Id));
                        UserSessionStorageService.Put(AdminInstitutionKey, adminInstitution);
                    }
                    else
                    {
                        UserSessionStorageService.Remove(AdminInstitutionKey);
                        adminInstitution = GetAdminInstitution(institutionId);
                    }
                }
            }

            return adminInstitution;
        }

        /// <param name="institutionId">Is used to reload the admin context</param>
        /// <param name="userId">Is used to reload the authentication context</param>
        public void ReloadAdminInstitution(int institutionId, int userId)
        {
            UserSessionStorageService.Remove(AdminInstitutionKey);
            var authResult = _authenticationServiceFactory().ReloadUser(userId);
            _authenticationContextFactory().Set(authResult.AuthenticatedInstitution);
            GetAdminInstitution(institutionId);
        }
    }
}