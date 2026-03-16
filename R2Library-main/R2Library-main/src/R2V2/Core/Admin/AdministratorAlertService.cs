#region

using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Admin
{
    public class AdministratorAlertService
    {
        private const string AllAlertsCacheKey = "Alerts.All";

        private static readonly object LockObject = new object();
        private readonly IQueryable<AdministratorAlert> _administratorAlert;
        private readonly IQueryable<AlertImage> _alertImages;
        private readonly IApplicationWideStorageService _applicationWideStorageService;
        private readonly ICartService _cartService;

        private readonly ILog<AdministratorAlertService> _log;
        private readonly IResourceService _resourceService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IQueryable<UserAlert> _userAlert;

        public AdministratorAlertService(
            ILog<AdministratorAlertService> log
            , IQueryable<AdministratorAlert> administratorAlert
            , IQueryable<UserAlert> userAlert
            , IUnitOfWorkProvider unitOfWorkProvider
            , IApplicationWideStorageService applicationWideStorageService
            , IQueryable<AlertImage> alertImages
            , ICartService cartService
            , IResourceService resourceService
        )
        {
            _log = log;
            _administratorAlert = administratorAlert;
            _userAlert = userAlert;
            _unitOfWorkProvider = unitOfWorkProvider;
            _applicationWideStorageService = applicationWideStorageService;
            _alertImages = alertImages;
            _cartService = cartService;
            _resourceService = resourceService;
        }

        public AlertCache GetAlertCache(bool forceReload)
        {
            if (forceReload)
            {
                ClearAlertsCache();
            }

            var alertCache = _applicationWideStorageService.Get<AlertCache>(AllAlertsCacheKey);
            if (alertCache == null)
            {
                _log.Debug("loading alerts cache");

                var alerts = GetAllAdminAlerts(false, 0);

                alertCache = new AlertCache(alerts);

                _log.DebugFormat("alerts cache loaded with {0} alerts", alerts.Count);

                _applicationWideStorageService.Put(AllAlertsCacheKey, alertCache);
            }

            return alertCache;
        }

        public List<AdministratorAlert> GetAllAdminAlerts(bool adminAreaAlerts, int year)
        {
            var adminAlerts = _administratorAlert
                .Fetch(x => x.AlertImages)
                .Fetch(x => x.Role)
                .Where(x => x.RecordStatus && (!adminAreaAlerts || x.CreationDate.Year == year));

            if (adminAreaAlerts)
            {
                return adminAlerts.ToList();
            }

            return adminAlerts.Where(x => x.StartDate.HasValue &&
                                          x.StartDate.Value.Date <= DateTime.Now.Date
                                          &&
                                          x.EndDate.HasValue &&
                                          x.EndDate.Value.Date >= DateTime.Now.Date).ToList();
        }

        public List<int> GetAllAdminAlertYears()
        {
            return _administratorAlert.Select(x => x.CreationDate.Year).Distinct().ToList();
        }

        public void ClearAlertsCache()
        {
            _log.Debug("waiting on lock to remove all alerts from the cache");
            lock (LockObject)
            {
                _applicationWideStorageService.Remove(AllAlertsCacheKey);
                _log.Debug("all alerts have been removed from the cache");
            }
        }

        public IAdminAlert GetAlertFromCache(int userId, int roleId, AuthenticatedInstitution institution)
        {
            IAdminAlert alert;
            using (_unitOfWorkProvider.Start(UnitOfWorkScope.NewOrCurrent))
            {
                var alertsReceived = roleId == (int)RoleCode.PUBUSER
                    ? _userAlert.Where(x => x.PublisherUserId == userId).Select(y => y.AlertId).ToList()
                    : _userAlert.Where(x => x.UserId == userId).Select(y => y.AlertId).ToList();


                var alertCache = GetAlertCache(false);

                var cart = roleId == (int)RoleCode.PUBUSER
                    ? null
                    : _cartService.GetInstitutionCartFromCache(institution.Id);

                var activeAndForthcomingResources = _resourceService.GetAllResources()
                    .Where(x => x.StatusId == (int)ResourceStatus.Active ||
                                x.StatusId == (int)ResourceStatus.Forthcoming).Select(x => x.Id).ToList();

                alert = alertCache.GetAlertWithExcludeAndRoleId(alertsReceived, roleId, institution, cart,
                    activeAndForthcomingResources);

                if (alert != null && alert.DisplayOnce && userId > 0)
                {
                    SaveUserAlert(userId, alert.Id, roleId == (int)RoleCode.PUBUSER);
                }
            }

            return alert;
        }

        public IAdminAlert GetAlertForEdit(int id)
        {
            var test = _administratorAlert.Fetch(x => x.AlertImages).Fetch(x => x.Role);
            var alert = test.Where(x => x.Id == id).ToList();

            return alert.FirstOrDefault() ?? new AdministratorAlert();
        }

        public IAdminAlert GetAlertToDelete(int id)
        {
            return _administratorAlert.FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<IAdminAlert> GetAllAlerts(int year)
        {
            return GetAllAdminAlerts(true, year);
        }

        public AlertImage GetAlertImage(int id)
        {
            return _alertImages.FirstOrDefault(x => x.Id == id);
        }

        public void SaveUserAlert(int userId, int alertId, bool isPublisher)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var userAlert = isPublisher
                            ? new UserAlert { AlertId = alertId, PublisherUserId = userId }
                            : new UserAlert { AlertId = alertId, UserId = userId };


                        _log.Debug(userAlert.ToDebugString());
                        uow.Save(userAlert);

                        uow.Commit();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _log.Error(ex.Message, ex);
                        throw;
                    }
                }
            }
        }
    }
}