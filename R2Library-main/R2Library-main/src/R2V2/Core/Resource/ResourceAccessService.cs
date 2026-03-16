#region

using System;
using System.Diagnostics;
using System.Linq;
using R2V2.Contexts;
using R2V2.Core.CollectionManagement;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Institution;
using R2V2.Infrastructure;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Resource
{
    public class ResourceAccessService : IResourceAccessService
    {
        private const string ResourceConcurrencySessionKey = "ResourceConcurrency.Session.Key";
        private readonly IAuthenticationContext _authenticationContext;
        private readonly ICartService _cartService;
        private readonly IContentSettings _contentSettings;

        private readonly ILog<ResourceAccessService> _log;
        private readonly PatronDrivenAcquisitionService _patronDrivenAcquisitionService;
        private readonly IRequestInformation _requestInformation;
        private readonly IQueryable<ResourceConcurrency> _resourceConcurrency;
        private readonly IResourceService _resourceService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IUserSessionStorageService _userSessionStorageService;

        public ResourceAccessService(ILog<ResourceAccessService> log
            , IAuthenticationContext authenticationContext
            , IQueryable<ResourceConcurrency> resourceConcurrency
            , IUnitOfWorkProvider unitOfWorkProvider
            , IRequestInformation requestInformation
            , IUserSessionStorageService userSessionStorageService
            , IContentSettings contentSettings
            , IResourceService resourceService
            , PatronDrivenAcquisitionService patronDrivenAcquisitionService
            , ICartService cartService
        )
        {
            _log = log;
            _authenticationContext = authenticationContext;
            _resourceConcurrency = resourceConcurrency;
            _unitOfWorkProvider = unitOfWorkProvider;
            _requestInformation = requestInformation;
            _userSessionStorageService = userSessionStorageService;
            _contentSettings = contentSettings;
            _resourceService = resourceService;
            _patronDrivenAcquisitionService = patronDrivenAcquisitionService;
            _cartService = cartService;
        }

        public ResourceAccess GetResourceAccess(string isbn)
        {
            var resource = _resourceService.GetAllResources().FirstOrDefault(x => x.Isbn == isbn);
            return resource != null ? GetResourceAccess(resource) : ResourceAccess.Denied;
        }

        public ResourceAccess GetResourceAccessForToc(string isbn)
        {
            var resource = _resourceService.GetAllResources().FirstOrDefault(x => x.Isbn == isbn);
            return resource != null ? GetResourceAccessForToc(resource) : ResourceAccess.Denied;
        }

        public bool IsPdaResource(string isbn)
        {
            var resource = _resourceService.GetAllResources().FirstOrDefault(x => x.Isbn == isbn);

            if (resource == null || !_authenticationContext.IsAuthenticated)
            {
                return false;
            }

            if (resource.IsArchive())
            {
                return false;
            }

            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            var license = authenticatedInstitution.GetResourceLicense(resource.Id);

            if (license != null && license.LicenseType == LicenseType.Pda && !license.PdaDeletedDate.HasValue)
            {
                return true;
            }

            return false;
        }


        public LicenseType GetLicenseType(int resourceId)
        {
            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            if (authenticatedInstitution == null)
            {
                return LicenseType.None;
            }

            var license = authenticatedInstitution.GetResourceLicense(resourceId);
            if (license == null)
            {
                return LicenseType.None;
            }

            return license.LicenseType;
        }

        public void ClearSessionResourceLocks()
        {
            var resourceConcurrency = RetrieveResourceConcurrencyIntoSession();
            if (resourceConcurrency == null)
            {
                return;
            }

            ClearResourceConcurrencyInSession();

            DeleteAllSessionIdResourceLocks(_requestInformation.SessionId);
        }

        public void CleanupResourceLocks()
        {
            _log.DebugFormat("CleanupResourceLocks() >>>");

            DeleteExpiredResourceLocks();

            _log.DebugFormat("CleanupResourceLocks() <<<");
        }


        public bool IsFullTextAvailable(int resourceId)
        {
            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            if (authenticatedInstitution != null)
            {
                var license = authenticatedInstitution.GetResourceLicense(resourceId);
                return IsFullTextAvailable(license);
            }

            return false;
        }

        public bool IsFullTextAvailable(License license)
        {
            if (license == null)
            {
                return false;
            }

            // sjs - 7/3/2013 - to fix an issue with PDA titles
            // once we have given access to a resource, there is not need to check again.
            // also, if the user has viewed a PDA resource already during this session, the access to the resource should remain for the remainder of the session.
            // not other user can gain access to the resource because the resource is no longer available as a PDA resource.  Concurrency is not an
            // issue because access is given based on the number of remaining views the PDA license has at the moment of access.
            if (HasInstitutionResourceLicenseLock(license.ResourceId, license.InstitutionId) ||
                _patronDrivenAcquisitionService.WasPdaResourcePreviouslyViewedThisSession(license.ResourceId))
            {
                return true;
            }

            // purchased licenses
            if (license.LicenseType == LicenseType.Purchased && license.LicenseCount > 0)
            {
                return true;
            }

            // trial licenses
            if (license.LicenseType == LicenseType.Trial && license.LicenseCount > 0)
            {
                return true;
            }

            // PDA licenses
            if (license.LicenseType == LicenseType.Pda &&
                license.PdaViewCount < license.PdaMaxViews &&
                license.PdaAddedToCartDate == null &&
                license.PdaDeletedDate == null
               )
            {
                // SJS - 2/4/2014 - added logic to restrict access to archived PDA resources
                // Issue #480 � PDA access to Archived Titles
                var resource = _resourceService.GetResource(license.ResourceId);
                if (resource.IsArchive())
                {
                    return false;
                }

                if (resource.NotSaleable)
                {
                    return false;
                }

                // is resource is active cart
                var cart = _cartService.GetInstitutionCartFromCache(license.InstitutionId);
                return cart == null || !cart.CartItems.Any(x => x.ResourceId == license.ResourceId);
            }

            return false;
        }

        private ResourceAccess GetResourceAccessForToc(IResource resource)
        {
            if (!_authenticationContext.IsAuthenticated)
            {
                return ResourceAccess.Denied;
            }

            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            if (authenticatedInstitution == null)
            {
                return ResourceAccess.Denied;
            }

            var license = authenticatedInstitution.GetResourceLicense(resource.Id);
            if (license == null || license.LicenseCount <= 0)
            {
                return ResourceAccess.Denied;
            }

            // SJS - 2/4/2014 - added logic to restrict access to archived PDA resources
            // Issue #480 � PDA access to Archived Titles
            if (license.LicenseType == LicenseType.Pda && resource.IsArchive())
            {
                return ResourceAccess.Denied;
            }

            //This will Prevent PDA access to PDA titles that have been manually deleted.
            if (license.LicenseType == LicenseType.Pda && (license.PdaDeletedDate != null || resource.NotSaleable))
            {
                return ResourceAccess.Denied;
            }


            return license.LicenseType == LicenseType.Pda && license.PdaViewCount >= license.PdaMaxViews
                ? ResourceAccess.Denied
                : ResourceAccess.Allowed;
        }


        private ResourceAccess GetResourceAccess(IResource resource)
        {
            if (!_authenticationContext.IsAuthenticated)
            {
                return ResourceAccess.Denied;
            }

            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            if (authenticatedInstitution == null)
            {
                return ResourceAccess.Denied;
            }

            var license = authenticatedInstitution.GetResourceLicense(resource.Id);
            if (license == null || license.LicenseCount <= 0)
            {
                return ResourceAccess.Denied;
            }

            // SJS - 2/4/2014 - added logic to restrict access to archived PDA resources
            // Issue #480 � PDA access to Archived Titles
            if (license.LicenseType == LicenseType.Pda && resource.IsArchive())
            {
                return ResourceAccess.Denied;
            }

            //This will Prevent PDA access to PDA titles that have been manually deleted.
            if (license.LicenseType == LicenseType.Pda && (license.PdaDeletedDate != null || resource.NotSaleable))
            {
                return ResourceAccess.Denied;
            }

            // check if session currently has a lock on the resource
            var hasInstitutionResourceLicense =
                HasInstitutionResourceLicenseLock(resource.Id, authenticatedInstitution.Id);
            if (hasInstitutionResourceLicense)
            {
                return ResourceAccess.Allowed;
            }

            // check if license for resource is available
            var isInstitutionResourceLicenseAvailable =
                IsInstitutionResourceLicenseAvailable(resource, authenticatedInstitution);
            if (!isInstitutionResourceLicenseAvailable)
            {
                return ResourceAccess.Locked;
            }

            if (!authenticatedInstitution.IsPublisherUser() && !authenticatedInstitution.IsSubscriptionUser())
            {
                IncrementUsage(resource.Id, authenticatedInstitution.Id);
            }

            return ResourceAccess.Allowed;
        }

        private bool HasInstitutionResourceLicenseLock(int resourceId, int institutionId)
        {
            var sessionResourceConcurrency = RetrieveResourceConcurrencyIntoSession();
            if (sessionResourceConcurrency == null)
            {
                return false;
            }

            if (sessionResourceConcurrency.InstitutionId == institutionId &&
                sessionResourceConcurrency.ResourceId == resourceId)
            {
                var minTime = DateTime.Now.AddSeconds(_contentSettings.ResourceLockTime * -1);
                if ((sessionResourceConcurrency.CreationDate > minTime &&
                     sessionResourceConcurrency.LastUpdated == null) ||
                    (sessionResourceConcurrency.LastUpdated != null &&
                     sessionResourceConcurrency.LastUpdated > minTime))
                {
                    UpdateResourceConcurrency(sessionResourceConcurrency.Id);
                    sessionResourceConcurrency.LastUpdated = DateTime.Now;
                    return true;
                }

                CleanupResourceLocks();
            }

            return false;
        }

        private bool IsInstitutionResourceLicenseAvailable(IResource resource,
            AuthenticatedInstitution authenticatedInstitution)
        {
            CleanupResourceLocks();
            return GetAvailableLicenseCount(resource, authenticatedInstitution) > 0;
        }

        private int GetAvailableLicenseCount(IResource resource, AuthenticatedInstitution authenticatedInstitution)
        {
            var availableLicenseCount = 0;

            try
            {
                // replace the above query with license counts already in the cache.
                var resourceLicense = authenticatedInstitution.GetResourceLicense(resource.Id);

                var licensesInUse = (from resourceConcurrency in _resourceConcurrency
                    where resourceConcurrency.ResourceId == resource.Id &&
                          resourceConcurrency.InstitutionId == authenticatedInstitution.Id
                    select resourceConcurrency).Count();

                availableLicenseCount = resourceLicense?.LicenseCount - licensesInUse ?? 0;
                _log.DebugFormat(
                    "GetAvailableLicenseCount(resourceId; {0}, authenticatedInstitution.Id: {1}) - availableLicenseCount: {2}, resourceLicense.LicenseCount: {3}, licensesInUse: {4}",
                    resource.Id, authenticatedInstitution.Id, availableLicenseCount, resourceLicense?.LicenseCount ?? 0,
                    licensesInUse);

                // patron driven acquisitions
                if (resourceLicense != null && availableLicenseCount > 0 &&
                    resourceLicense.LicenseType == LicenseType.Pda)
                {
                    if (_patronDrivenAcquisitionService.UpdatePartonDrivenAcquisitionView(resource,
                            authenticatedInstitution))
                    {
                        resourceLicense.PdaViewCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            _log.DebugFormat(
                "GetAvailableLicenseCount(resourceId: {0}, authenticatedInstitution: {1}) - availableLicenseCount: {2}",
                resource.Id, authenticatedInstitution.Id, availableLicenseCount);
            return availableLicenseCount;
        }

        private void IncrementUsage(int resourceId, int institutionId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var resourceConcurrency = new ResourceConcurrency
                    {
                        SessionId = _requestInformation.SessionId, ResourceId = resourceId,
                        InstitutionId = institutionId, LastUpdated = DateTime.Now
                    };
                    StoreResourceConcurrencyIntoSession(resourceConcurrency);

                    uow.SaveOrUpdate(resourceConcurrency);
                    uow.Commit();
                    transaction.Commit();

                    uow.Evict(resourceConcurrency);
                }
            }
        }

        private void UpdateResourceConcurrency(int resourceConcurrenyId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;

                string updatedBy;
                if (authenticatedInstitution == null)
                {
                    _log.Error("UpdateResourceConcurrency() - AuthenticatedInstitution is null");
                    updatedBy = "null";
                }
                else
                {
                    updatedBy = authenticatedInstitution.AuditId;
                }

                const string update =
                    "update tResourceConcurreny set vchUpdaterId = :UpdateId, dtLastUpdate = :UpdateTime where iResourceConcurrenyId = :ResourceConcurrenyId";
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var query = uow.Session.CreateSQLQuery(update);
                query.SetParameter("UpdateId", updatedBy);
                query.SetParameter("ResourceConcurrenyId", resourceConcurrenyId);
                query.SetParameter("UpdateTime", DateTime.Now);
                var rows = query.ExecuteUpdate();
                stopwatch.Stop();
                _log.DebugFormat(
                    "UpdateResourceConcurrency() - rows: {0}, resourceConcurrenyId: {1}, updateId: {2}, time: {3} ms",
                    rows, resourceConcurrenyId, updatedBy, stopwatch.ElapsedMilliseconds);
            }
        }

        private void DeleteAllSessionIdResourceLocks(string sessionId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                const string update = "delete from tResourceConcurreny where vchSessionId = :SessionId";
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var query = uow.Session.CreateSQLQuery(update);
                query.SetParameter("SessionId", sessionId);
                var rows = query.ExecuteUpdate();
                stopwatch.Stop();
                _log.DebugFormat("DeleteAllSessionIdResourceLocks() - rows: {0}, sessionId: {1}, time: {2} ms", rows,
                    sessionId, stopwatch.ElapsedMilliseconds);
            }
        }

        private void DeleteExpiredResourceLocks()
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                var minDate = DateTime.Now.AddSeconds(_contentSettings.ResourceLockTime * -1);
                const string update =
                    "delete from tResourceConcurreny where (dtCreationDate < :MinDate and dtLastUpdate is null) or (dtLastUpdate is not null and dtLastUpdate < :MinDate)";
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var query = uow.Session.CreateSQLQuery(update);
                query.SetParameter("MinDate", minDate);
                var rows = query.ExecuteUpdate();
                stopwatch.Stop();
                _log.DebugFormat("DeleteExpiredResourceLocks() - rows: {0}, minDate: {1}, time: {2} ms", rows, minDate,
                    stopwatch.ElapsedMilliseconds);
            }
        }


        private void StoreResourceConcurrencyIntoSession(ResourceConcurrency resourceConcurrency)
        {
            _log.DebugFormat(
                "StoreResourceConcurrencyIntoSession() - InstitutionId: {0}, ResourceId: {1}, SessionId: {2}",
                resourceConcurrency.InstitutionId, resourceConcurrency.ResourceId, resourceConcurrency.SessionId);
            _userSessionStorageService.Put(ResourceConcurrencySessionKey, resourceConcurrency);
        }

        private ResourceConcurrency RetrieveResourceConcurrencyIntoSession()
        {
            var resourceConcurrency =
                _userSessionStorageService.Get<ResourceConcurrency>(ResourceConcurrencySessionKey);
            return resourceConcurrency;
        }

        private void ClearResourceConcurrencyInSession()
        {
            var resourceConcurrency =
                _userSessionStorageService.Get<ResourceConcurrency>(ResourceConcurrencySessionKey);
            if (resourceConcurrency != null)
            {
                _log.DebugFormat(
                    "ClearResourceConcurrencyInSession() - InstitutionId: {0}, ResourceId: {1}, SessionId: {2}",
                    resourceConcurrency.InstitutionId, resourceConcurrency.ResourceId, resourceConcurrency.SessionId);
            }
            else
            {
                _log.Debug("ClearResourceConcurrencyInSession() - null");
            }

            _userSessionStorageService.Remove(ResourceConcurrencySessionKey);
        }
    }
}