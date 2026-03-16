#region

using System;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Storages;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using AdministratorAlert = R2V2.Web.Areas.Admin.Models.Alerts.AdministratorAlert;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public class AlertBuildFilter : R2V2ResultFilter
    {
        private const string AlertDisplayedKey = "Alert.AlertDisplayed";
        private readonly Func<AdministratorAlertService> _administratorAlertServiceFactory;
        private readonly IAdminSettings _adminSettings;

        private readonly IAuthenticationContext _authenticationContext;
        private readonly Func<ResourceDiscountService> _resourceDiscountServiceFactory;

        private readonly Func<IResourceService> _resourceServiceFactory;
        private readonly Func<IUserSessionStorageService> _userSessionStorageServiceFactory;

        public AlertBuildFilter(IAuthenticationContext authenticationContext
            , IAdminSettings adminSettings
            , Func<AdministratorAlertService> administratorAlertServiceFactory
            , Func<IUserSessionStorageService> userSessionStorageService
            , Func<IResourceService> resourceServiceFactory
            , Func<ResourceDiscountService> resourceDiscountServiceFactory
        )
            : base(authenticationContext)
        {
            _authenticationContext = authenticationContext;
            _adminSettings = adminSettings;
            _administratorAlertServiceFactory = administratorAlertServiceFactory;
            _userSessionStorageServiceFactory = userSessionStorageService;
            _resourceServiceFactory = resourceServiceFactory;
            _resourceDiscountServiceFactory = resourceDiscountServiceFactory;
        }

        private IUserSessionStorageService UserSessionStorageService => _userSessionStorageServiceFactory();
        private ResourceDiscountService ResourceDiscountService => _resourceDiscountServiceFactory();
        private IResourceService ResourceService => _resourceServiceFactory();
        private AdministratorAlertService AdministratorAlertService => _administratorAlertServiceFactory();


        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var model = filterContext.Controller.ViewData.Model;
            var baseModel = model as IR2V2Model;

            if (baseModel == null)
            {
                return;
            }

            var isAuthenticated = _authenticationContext.IsAuthenticated;

            var alertObject = UserSessionStorageService.Get(AlertDisplayedKey);

            var sessionAlertDisplayed = alertObject != null && (bool)alertObject;

            //Get alert only if they are authenicated. 
            //CANNOT display add with IP authenicated because if they login after they will not recieve an alert.
            if (isAuthenticated && !sessionAlertDisplayed)
            {
                int roleId;

                if (_authenticationContext.IsSalesAssociate())
                {
                    roleId = (int)RoleCode.SALESASSOC;
                }
                else if (_authenticationContext.IsInstitutionAdmin())
                {
                    roleId = (int)RoleCode.INSTADMIN;
                }
                else if (_authenticationContext.IsRittenhouseAdmin())
                {
                    roleId = (int)RoleCode.RITADMIN;
                }
                else if (_authenticationContext.IsPublisherUser())
                {
                    roleId = (int)RoleCode.PUBUSER;
                }
                else if (_authenticationContext.IsInstitutionUser())
                {
                    roleId = (int)RoleCode.USERS;
                }
                else
                {
                    return;
                }

                //Prevents the alert from poping up again in this session.
                UserSessionStorageService.Put(AlertDisplayedKey, true);


                //Gets the alert based off the user and there role. 
                var administratorAlert = AdministratorAlertService.GetAlertFromCache(UserId, roleId,
                    AuthenticationContext.AuthenticatedInstitution);

                //Show the alert if one exists. 
                if (administratorAlert != null)
                {
                    IResource resource = null;
                    if (administratorAlert.ResourceId.HasValue)
                    {
                        resource = ResourceService.GetAllResources()
                            .FirstOrDefault(x => x.Id == administratorAlert.ResourceId);
                    }

                    var alert = new AdministratorAlert(administratorAlert, _adminSettings.AlertImageLocation,
                        AuthenticationContext.AuthenticatedInstitution, resource);
                    if (alert.Resource != null)
                    {
                        var collectionManagementResource = new CollectionManagementResource
                            { Resource = alert.Resource };
                        var institution = new AdminInstitution(AuthenticationContext.AuthenticatedInstitution);
                        ResourceDiscountService.SetDiscount(collectionManagementResource, institution);
                        alert.DiscountPrice = collectionManagementResource.DiscountPrice;

                        //TODO: Hide PDA button if they have Already PDAed it
                        var pdaLicense = AuthenticationContext.AuthenticatedInstitution.Licenses.FirstOrDefault(x =>
                            x.LicenseType == LicenseType.Pda && x.ResourceId == alert.ResourceId);
                        if (pdaLicense != null)
                        {
                            alert.DisplayPDA = false;
                        }
                    }

                    baseModel.Alert = alert;
                }
            }
        }

        private bool CanPurchase()
        {
            if (_authenticationContext.IsSalesAssociate())
            {
                return true;
            }

            if (_authenticationContext.IsInstitutionAdmin())
            {
                return true;
            }

            if (_authenticationContext.IsRittenhouseAdmin())
            {
                return true;
            }

            if (_authenticationContext.IsPublisherUser())
            {
                return true;
            }

            return false;
        }
    }
}