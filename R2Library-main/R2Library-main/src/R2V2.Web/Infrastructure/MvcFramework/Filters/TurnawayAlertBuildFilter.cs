#region

using System;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Storages;
using R2V2.Web.Areas.Admin.Models.Alerts;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public class TurnawayAlertBuildFilter : R2V2ResultFilter
    {
        private const string TurnawayAlertDisplayedKey = "TurnawayAlert.AlertDisplayed";

        private readonly IAuthenticationContext _authenticationContext;
        private readonly Func<TurnawayAlertService> _turnawayAlertService;
        private readonly Func<IUserSessionStorageService> _userSessionStorageService;
        private readonly IWebSettings _webSettings;

        public TurnawayAlertBuildFilter(
            IAuthenticationContext authenticationContext
            , Func<TurnawayAlertService> turnawayAlertService
            , Func<IUserSessionStorageService> userSessionStorageService
            , IWebSettings webSettings
        )
            : base(authenticationContext)
        {
            _authenticationContext = authenticationContext;
            _turnawayAlertService = turnawayAlertService;
            _userSessionStorageService = userSessionStorageService;
            _webSettings = webSettings;
        }

        private IUserSessionStorageService UserSessionStorageService => _userSessionStorageService();

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var model = filterContext.Controller.ViewData.Model;
            var baseModel = model as IR2V2Model;

            if (baseModel == null || !ShouldAlertBeProcessed(baseModel))
            {
                return;
            }

            var user = _authenticationContext.AuthenticatedInstitution.User;

            var lastAlert = user.ConcurrentTurnawayAlert;

            var isTestAccount = _authenticationContext.AuthenticatedInstitution.AccountNumber == "005034";

            if (isTestAccount && !string.IsNullOrWhiteSpace(_webSettings.EnvironmentName))
            {
                lastAlert = new DateTime(2014, 07, 01);
            }

            if (lastAlert.HasValue && lastAlert.Value > DateTime.Now.AddDays(-30))
            {
                return;
            }

            var turnawayAlertService = _turnawayAlertService();

            var concurrentTurnawayResourceCount =
                turnawayAlertService.GetConcurrentTurnawayResourceCount(lastAlert,
                    user.InstitutionId.GetValueOrDefault());

            if (concurrentTurnawayResourceCount == 0)
            {
                return;
            }

            var concurrentTurnawayAlert = new TurnawayAlert(user.InstitutionId.GetValueOrDefault(),
                concurrentTurnawayResourceCount, lastAlert);
            baseModel.ConcurrentTurnawayAlert = concurrentTurnawayAlert;

            //Prevents the alert from poping up again in this session.
            UserSessionStorageService.Put(TurnawayAlertDisplayedKey, true);

            if (!isTestAccount)
            {
                turnawayAlertService.UpdateUserConcurrentTurnawayDate(user.Id, DateTime.Now);
            }
        }

        private bool ShouldAlertBeProcessed(IR2V2Model baseModel)
        {
            var alertObject = UserSessionStorageService.Get(TurnawayAlertDisplayedKey);


            if (alertObject != null && (bool)alertObject)
            {
                return false;
            }

            if (_authenticationContext.AuthenticatedInstitution == null)
            {
                return false;
            }

            if (_authenticationContext.AuthenticatedInstitution.User == null)
            {
                return false;
            }

            if (!_authenticationContext.IsInstitutionAdmin())
            {
                return false;
            }

            return true;
        }
    }
}