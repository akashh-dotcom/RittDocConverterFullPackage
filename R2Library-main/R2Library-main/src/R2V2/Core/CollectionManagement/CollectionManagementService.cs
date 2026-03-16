#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Contexts;
using R2V2.Core.Admin;
using R2V2.Core.Institution;
using R2V2.Core.Publisher;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;
using InstitutionResourceLicense = R2V2.Core.Institution.InstitutionResourceLicense;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class CollectionManagementService : ICollectionManagementService
    {
        private readonly IAdminContext _adminContext;
        private readonly IFeaturedTitleService _featuredTitleService;
        private readonly InstitutionResourceAuditFactory _institutionResourceAuditFactory;
        private readonly IQueryable<InstitutionResourceLicense> _institutionResourceLicenses;
        private readonly ILog<CollectionManagementService> _log;
        private readonly PublisherService _publisherService;
        private readonly RecommendationsService _recommendationsService;
        private readonly ResourceDiscountService _resourceDiscountService;
        private readonly IResourceService _resourceService;
        private readonly TurnawayAlertService _turnawayAlertService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        /// <param name="featuredTitleService"> </param>
        public CollectionManagementService(ILog<CollectionManagementService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IResourceService resourceService
            , IAdminContext adminContext
            , PublisherService publisherService
            , IQueryable<InstitutionResourceLicense> institutionResourceLicenses
            , InstitutionResourceAuditFactory institutionResourceAuditFactory
            , IFeaturedTitleService featuredTitleService
            , RecommendationsService recommendationsService
            , TurnawayAlertService turnawayAlertService
            , ResourceDiscountService resourceDiscountService
        )
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _resourceService = resourceService;
            _adminContext = adminContext;
            _publisherService = publisherService;
            _institutionResourceLicenses = institutionResourceLicenses;
            _institutionResourceAuditFactory = institutionResourceAuditFactory;
            _featuredTitleService = featuredTitleService;
            _recommendationsService = recommendationsService;
            _turnawayAlertService = turnawayAlertService;
            _resourceDiscountService = resourceDiscountService;
        }

        public CollectionManagementResource GetCollectionManagementResource(int institutionId, int resourceId,
            CachedCart cart)
        {
            var resource = _resourceService.GetResource(resourceId);
            return GetCollectionManagementResource(institutionId, resource, cart);
        }

        public CollectionManagementResource GetCollectionManagementResource(int institutionId, string isbn,
            CachedCart cart)
        {
            var resource = _resourceService.GetResource(isbn);
            return GetCollectionManagementResource(institutionId, resource, cart);
        }

        public CollectionManagementResource GetCollectionManagementResourceWithoutDatabase(int institutionId,
            string isbn, CachedCart cart)
        {
            var resource = _resourceService.GetResourceWithoutDatabase(isbn);
            return GetCollectionManagementResource(institutionId, resource, cart);
        }


        public IEnumerable<CollectionManagementResource> GetCollectionManagementResources(
            ICollectionManagementQuery collectionManagementQuery, IOrder order, bool isExpertReviewer = false)
        {
            var institution = _adminContext.GetAdminInstitution(order.InstitutionId);

            var resources = _resourceService.GetResources(collectionManagementQuery,
                _publisherService.GetFeaturedPublisher(), _featuredTitleService.GetFeaturedTitles(),
                _publisherService.GetPublishers());

            var resourceIdsAndCount = new Dictionary<int, int>();
            if (collectionManagementQuery.TurnawayStartDate.HasValue)
            {
                resourceIdsAndCount = _turnawayAlertService.GetConcurrentTurnawayResourceIdsAndCount(
                    collectionManagementQuery.TurnawayStartDate, collectionManagementQuery.InstitutionId);
                if (resourceIdsAndCount.Any())
                {
                    resources = resources.TurnawayResourcesFilterBy(resourceIdsAndCount);
                }
            }

            var collectionManagementResources = GetInstitutionCollectionManagementResources(resources, order,
                institution, collectionManagementQuery.RecommendationsOnly, isExpertReviewer);

            if (resourceIdsAndCount != null && resourceIdsAndCount.Any())
            {
                foreach (var collectionManagementResource in collectionManagementResources)
                {
                    collectionManagementResource.ConcurrentTurnawayCount =
                        resourceIdsAndCount[collectionManagementResource.Resource.Id];
                }
            }

            if (collectionManagementQuery.PdaStatus != PdaStatus.None)
            {
                collectionManagementResources =
                    FilterByPdaStatus(collectionManagementResources, collectionManagementQuery.PdaStatus);
            }

            if (collectionManagementQuery.IncludePdaResources)
            {
                collectionManagementResources = FilterByPdaCreateDate(collectionManagementResources,
                    collectionManagementQuery.PdaDateAddedMin,
                    collectionManagementQuery.PdaDateAddedMax);
            }

            if (collectionManagementQuery.PurchasedOnly)
            {
                if (institution.AccountStatus.Id == AccountStatus.Active)
                {
                    var list = collectionManagementResources
                        .Where(x => x.LicenseCount > 0 && x.LicenseType == LicenseType.Purchased).ToList();
                    return list.OrderBy(collectionManagementQuery);
                }

                return new List<CollectionManagementResource>();
            }

            if (collectionManagementQuery.IncludePdaResources)
            {
                if (institution.AccountStatus.Id == AccountStatus.Active)
                {
                    if (collectionManagementQuery.IncludePdaHistory)
                    {
                        return collectionManagementResources
                            .Where(x => x.OriginalSource == LicenseOriginalSource.Pda)
                            .OrderBy(collectionManagementQuery);
                    }

                    return collectionManagementResources
                        .Where(x => x.LicenseType == LicenseType.Pda &&
                                    (x.Resource.StatusId == (int)ResourceStatus.Active ||
                                     x.Resource.StatusId == (int)ResourceStatus.Forthcoming))
                        .Where(x => x.PdaAddedToCartDate == null && x.CartLicenseCount == 0)
                        .Where(x => x.PdaDeletedDate == null)
                        .Where(x => !x.Resource.NotSaleable)
                        .OrderBy(collectionManagementQuery);
                }

                return new List<CollectionManagementResource>();
            }

            return collectionManagementResources
                .Where(x => !x.Resource.NotSaleable)
                .OrderBy(collectionManagementQuery);
        }


        public IEnumerable<CollectionManagementResource> GetCollectionManagementResources(
            IEnumerable<IResource> filteredResources, ICollectionManagementQuery collectionManagementQuery,
            IOrder order, bool isExpertReviewer = false)
        {
            var institution = _adminContext.GetAdminInstitution(order.InstitutionId);

            var resources = _resourceService.GetResources(filteredResources, collectionManagementQuery,
                _publisherService.GetFeaturedPublisher(), _featuredTitleService.GetFeaturedTitles());

            var resourceIdsAndCount = new Dictionary<int, int>();
            if (collectionManagementQuery.TurnawayStartDate.HasValue)
            {
                resourceIdsAndCount = _turnawayAlertService.GetConcurrentTurnawayResourceIdsAndCount(
                    collectionManagementQuery.TurnawayStartDate, collectionManagementQuery.InstitutionId);
                if (resourceIdsAndCount.Any())
                {
                    resources = resources.TurnawayResourcesFilterBy(resourceIdsAndCount);
                }
            }

            var collectionManagementResources = GetInstitutionCollectionManagementResources(resources, order,
                institution, collectionManagementQuery.RecommendationsOnly, isExpertReviewer);

            if (resourceIdsAndCount != null && resourceIdsAndCount.Any())
            {
                foreach (var collectionManagementResource in collectionManagementResources)
                {
                    collectionManagementResource.ConcurrentTurnawayCount =
                        resourceIdsAndCount[collectionManagementResource.Resource.Id];
                }
            }

            if (collectionManagementQuery.PdaStatus != PdaStatus.None)
            {
                collectionManagementResources =
                    FilterByPdaStatus(collectionManagementResources, collectionManagementQuery.PdaStatus);
            }

            if (collectionManagementQuery.IsReserveShelf)
            {
                if (collectionManagementQuery.PurchasedOnly)
                {
                    if (institution.AccountStatus.Id == AccountStatus.Active)
                    {
                        var list = collectionManagementResources
                            .Where(x => x.LicenseCount > 0 && x.LicenseType == LicenseType.Purchased).ToList();
                        return list.OrderBy(collectionManagementQuery);
                    }

                    return new List<CollectionManagementResource>();
                }

                return collectionManagementResources
                    .Where(x => (
                                    x.LicenseType == LicenseType.Pda && (
                                        x.Resource.StatusId == (int)ResourceStatus.Active
                                        ||
                                        x.Resource.StatusId == (int)ResourceStatus.Forthcoming
                                    ) &&
                                    x.PdaAddedToCartDate == null && x.CartLicenseCount == 0 &&
                                    x.PdaDeletedDate == null &&
                                    !x.Resource.NotSaleable
                                )
                                || x.LicenseType == LicenseType.Purchased
                    )
                    .OrderBy(collectionManagementQuery);
            }


            if (collectionManagementQuery.IncludePdaResources)
            {
                collectionManagementResources = FilterByPdaCreateDate(collectionManagementResources,
                    collectionManagementQuery.PdaDateAddedMin,
                    collectionManagementQuery.PdaDateAddedMax);
            }

            if (collectionManagementQuery.PurchasedOnly)
            {
                if (institution.AccountStatus.Id == AccountStatus.Active)
                {
                    var list = collectionManagementResources
                        .Where(x => x.LicenseCount > 0 && x.LicenseType == LicenseType.Purchased).ToList();
                    return list.OrderBy(collectionManagementQuery);
                }

                return new List<CollectionManagementResource>();
            }

            if (collectionManagementQuery.IncludePdaResources)
            {
                if (institution.AccountStatus.Id == AccountStatus.Active)
                {
                    if (collectionManagementQuery.IncludePdaHistory)
                    {
                        return collectionManagementResources
                            .Where(x => x.OriginalSource == LicenseOriginalSource.Pda)
                            .OrderBy(collectionManagementQuery);
                    }

                    return collectionManagementResources
                        .Where(x => x.LicenseType == LicenseType.Pda &&
                                    (x.Resource.StatusId == (int)ResourceStatus.Active ||
                                     x.Resource.StatusId == (int)ResourceStatus.Forthcoming))
                        .Where(x => x.PdaAddedToCartDate == null && x.CartLicenseCount == 0)
                        .Where(x => x.PdaDeletedDate == null)
                        .Where(x => !x.Resource.NotSaleable)
                        .OrderBy(collectionManagementQuery);
                }

                return new List<CollectionManagementResource>();
            }

            return collectionManagementResources
                //.Where(x => !x.Resource.NotSaleable)
                .OrderBy(collectionManagementQuery);
        }


        public IEnumerable<CollectionManagementResource> GetCollectionManagementResourcesExcludeIds(
            ICollectionManagementQuery collectionManagementQuery, IOrder order, int[] resourceIds)
        {
            //TODO: Need to change this.
            var institution = _adminContext.GetAdminInstitution(order.InstitutionId);

            var publishers = _publisherService.GetPublishers();

            //IEnumerable<IResource> filteredResources


            var resources =
                _resourceService.GetResourcesExcludeIds(collectionManagementQuery, resourceIds, publishers)
                    .Where(x => !x.IsDisabled());
            //List<CollectionManagementResource> collectionManagementResources = (from resource in resources
            //    let specialDiscountResource = _specialDiscountResourceFactory.GetSpecialResourceDiscount(resource.Id)
            //    select
            //        new CollectionManagementResource(resource, institution.Discount, order.PromotionDiscount,
            //            specialDiscountResource)).ToList();
            var collectionManagementResources = resources
                .Select(resource => new CollectionManagementResource(resource, order.OrderId)).ToList();

            foreach (var collectionManagementResource in collectionManagementResources)
            {
                _resourceDiscountService.SetDiscount(collectionManagementResource, institution);

                var resourceLicense =
                    institution.Licenses.SingleOrDefault(x => x.ResourceId == collectionManagementResource.Resource.Id);
                if (resourceLicense != null)
                {
                    collectionManagementResource.LicenseCount = resourceLicense.LicenseCount;
                }

                collectionManagementResource.CartLicenseCount =
                    order.Items.Where(x => x.Id == collectionManagementResource.Resource.Id)
                        .Sum(x => x.NumberOfLicenses);
            }

            return collectionManagementResources
                .Where(x => (!collectionManagementQuery.PurchasedOnly && !x.Resource.NotSaleable) ||
                            (collectionManagementQuery.PurchasedOnly && x.LicenseCount > 0))
                .OrderBy(collectionManagementQuery);
        }

        public bool UpdateInstitutionResourceLicenses(int institutionId, int resourceId, int numberOfLicenses)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        var license =
                            _institutionResourceLicenses.SingleOrDefault(ir =>
                                ir.InstitutionId == institutionId && ir.ResourceId == resourceId);

                        if (license == null)
                        {
                            uow.Commit();
                            transaction.Rollback();
                            _log.ErrorFormat(
                                "UpdateInstitutionResourceLicenses(institutionId: {0}, resourceId: {1}, numberOfLicenses: {2}) - Institution Resource License (tInstitutionResourceLicense) not found",
                                institutionId, resourceId, numberOfLicenses);
                            return false;
                        }

                        license.LicenseCount = numberOfLicenses;

                        var audit =
                            _institutionResourceAuditFactory.BuildAuditRecord(
                                InstitutionResourceAuditType.ResourceLicenceCountUpdated,
                                institutionId, resourceId, numberOfLicenses);
                        _log.DebugFormat("Inserting - {0}", license);
                        _log.DebugFormat("Inserting - {0}", audit);
                        uow.Update(license);
                        uow.Save(audit);

                        uow.Commit();
                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                // throw ex;
                return false;
            }

            return true;
        }

        public bool UpdateFreeLicenses(int resourceId, bool isFreeResource)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        var licenses = _institutionResourceLicenses.Where(x => x.ResourceId == resourceId);

                        foreach (var institutionResourceLicense in licenses)
                        {
                            if (isFreeResource)
                            {
                                institutionResourceLicense.LicenseCount += 500;
                            }
                            else
                            {
                                institutionResourceLicense.LicenseCount -= 500;
                                if (institutionResourceLicense.LicenseCount <= 0)
                                {
                                    institutionResourceLicense.LicenseCount = 1;
                                }
                            }

                            uow.Update(institutionResourceLicense);

                            var audit =
                                _institutionResourceAuditFactory.BuildAuditRecord(
                                    InstitutionResourceAuditType.ResourceLicenceCountUpdated,
                                    institutionResourceLicense.InstitutionId,
                                    institutionResourceLicense.ResourceId,
                                    institutionResourceLicense.LicenseCount);

                            uow.Save(audit);
                        }

                        uow.Commit();
                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                // throw ex;
                return false;
            }

            return true;
        }

        private CollectionManagementResource GetCollectionManagementResource(int institutionId, IResource resource,
            CachedCart cart)
        {
            if (resource == null)
            {
                return null;
            }

            var institution = _adminContext.GetAdminInstitution(institutionId);
            var license = GetInstitutionResourceLicense(institutionId, resource.Id);

            var cartLicenseCount = cart != null
                ? cart.CartItems.Where(x => x.ResourceId == resource.Id).Sum(x => x.NumberOfLicenses)
                : 0;

            var collectionManagementResource = new CollectionManagementResource(resource, cart != null ? cart.Id : 0);
            if (license == null)
            {
                collectionManagementResource.LicenseCount = 0;
                collectionManagementResource.LicenseType = LicenseType.None;
                collectionManagementResource.CartLicenseCount = cartLicenseCount;
            }

            if (license != null)
            {
                _log.DebugFormat("GetCollectionManagementResource() - {0}", license.ToDebugString());
                collectionManagementResource.LicenseCount = license.LicenseCount;
                collectionManagementResource.LicenseType = license.LicenseType;
                collectionManagementResource.CartLicenseCount = cartLicenseCount;
            }

            _resourceDiscountService.SetDiscount(collectionManagementResource, institution);
            return collectionManagementResource;
        }

        private License GetInstitutionResourceLicense(int institutionId, int resourceId)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);
            return adminInstitution.GetLicense(resourceId);
        }

        /// <summary>
        ///     Used for Dashboard only for right now.
        /// </summary>
        public IEnumerable<CollectionManagementResource> GetCollectionManagementResources(int[] resourceIds,
            IOrder order, bool recommendationsOnly = false)
        {
            var institution = _adminContext.GetAdminInstitution(order.InstitutionId);

            //TODO: Make sure RecentOnly will allow only the Ids passed in will be returned
            var resources = _resourceService.GetResources(new ResourceQuery { RecentOnly = true },
                _publisherService.GetFeaturedPublisher(), _featuredTitleService.GetFeaturedTitles(), false, resourceIds,
                _publisherService.GetPublishers());

            var collectionManagementResources =
                GetInstitutionCollectionManagementResources(resources, order, institution, recommendationsOnly);

            return collectionManagementResources;
        }

        private List<CollectionManagementResource> GetInstitutionCollectionManagementResources(
            IEnumerable<IResource> resources, IOrder order, IAdminInstitution institution,
            bool recommendationOnly = false, bool isExpertReviewer = false)
        {
            var resourceList = resources?.ToList();
            var collectionManagementResources = new List<CollectionManagementResource>();
            if (resourceList != null)
            {
                if (recommendationOnly)
                {
                    var recommendations = isExpertReviewer
                        ? _recommendationsService.GetRecommendationsIncludeDeleted(order.InstitutionId)
                        : _recommendationsService.GetRecommendations(order.InstitutionId);

                    resourceList = resourceList.Where(x => recommendations.Any(y => y.ResourceId == x.Id)).ToList();
                }

                foreach (var newCollectionManagementResources in resourceList.Select(resource =>
                             new CollectionManagementResource(resource, order.OrderId)))
                {
                    _resourceDiscountService.SetDiscount(newCollectionManagementResources, institution);
                    collectionManagementResources.Add(newCollectionManagementResources);
                }
            }

            foreach (var collectionManagementResource in collectionManagementResources)
            {
                var resourceLicenses = institution.Licenses
                    .Where(x => x.ResourceId == collectionManagementResource.Resource.Id).ToList();
                if (resourceLicenses.Any())
                {
                    var resourceLicense = resourceLicenses.First();
                    if (resourceLicense != null)
                    {
                        collectionManagementResource.LicenseType = resourceLicense.LicenseType;
                        var resource = resourceList?.FirstOrDefault(x => x.Id == resourceLicense.ResourceId);
                        if (resourceLicense.LicenseType == LicenseType.Purchased)
                        {
                            collectionManagementResource.LicenseCount = resourceLicense.LicenseCount;
                            collectionManagementResource.FirstPurchaseDate = resourceLicense.FirstPurchaseDate;
                            if (resourceLicense.OriginalSource == LicenseOriginalSource.Pda)
                            {
                                collectionManagementResource.PdaAddedDate = resourceLicense.PdaAddedDate;
                                collectionManagementResource.PdaAddedToCartDate = resourceLicense.PdaAddedToCartDate;
                                collectionManagementResource.PdaCartDeletedDate = resourceLicense.PdaCartDeletedDate;
                                collectionManagementResource.PdaCartDeletedByName =
                                    resourceLicense.PdaCartDeletedByName;
                                collectionManagementResource.PdaViewCount = resourceLicense.PdaViewCount;
                                collectionManagementResource.PdaMaxViews = resourceLicense.PdaMaxViews;
                                collectionManagementResource.ResourceNotSaleableDate = resource?.NotSaleableDate;
                                collectionManagementResource.PdaDeletedDate = resourceLicense.PdaDeletedDate;
                                collectionManagementResource.PdaRuleAddedDate = resourceLicense.PdaRuleAddedDate;
                            }
                        }
                        else if (resourceLicense.LicenseType == LicenseType.Pda)
                        {
                            collectionManagementResource.LicenseCount = 0;
                            collectionManagementResource.PdaAddedDate = resourceLicense.PdaAddedDate;
                            collectionManagementResource.PdaAddedToCartDate = resourceLicense.PdaAddedToCartDate;
                            collectionManagementResource.PdaCartDeletedDate = resourceLicense.PdaCartDeletedDate;
                            collectionManagementResource.PdaCartDeletedByName = resourceLicense.PdaCartDeletedByName;
                            collectionManagementResource.PdaViewCount = resourceLicense.PdaViewCount;
                            collectionManagementResource.PdaMaxViews = resourceLicense.PdaMaxViews;
                            collectionManagementResource.ResourceNotSaleableDate = resource?.NotSaleableDate;
                            collectionManagementResource.PdaDeletedDate = resourceLicense.PdaDeletedDate;
                            collectionManagementResource.PdaRuleAddedDate = resourceLicense.PdaRuleAddedDate;
                        }

                        collectionManagementResource.OriginalSource = resourceLicense.OriginalSource;
                    }
                }

                collectionManagementResource.CartLicenseCount = order.Items
                    .Where(x => x.Id == collectionManagementResource.Resource.Id).Sum(x => x.NumberOfLicenses);

                //The Resource Order Item NumberOfLicenses will be 0 if it is a a free resource
                collectionManagementResource.FreeLicenseInCart = collectionManagementResource.Resource.IsFreeResource &&
                                                                 //collectionManagementResource.CartLicenseCount == 0 &&
                                                                 order.Items.Any(x =>
                                                                     x.Id ==
                                                                     collectionManagementResource.Resource.Id);
            }

            return collectionManagementResources;
        }

        private List<CollectionManagementResource> FilterByPdaCreateDate(List<CollectionManagementResource> resources,
            DateTime? minDate, DateTime? maxDate)
        {
            if (minDate == null && maxDate == null)
            {
                return resources;
            }

            var min = minDate == null
                ? DateTime.MinValue
                : new DateTime(minDate.Value.Year, minDate.Value.Month, minDate.Value.Day, 0, 0, 0, 0);
            var max = maxDate == null
                ? DateTime.MaxValue
                : new DateTime(maxDate.Value.Year, maxDate.Value.Month, maxDate.Value.Day, 23, 59, 59, 999);
            return resources.Where(x => x.PdaAddedDate >= min && x.PdaAddedDate <= max).ToList();
        }

        private List<CollectionManagementResource> FilterByPdaStatus(List<CollectionManagementResource> resources,
            PdaStatus pdaStatus)
        {
            switch (pdaStatus)
            {
                case PdaStatus.Active:
                    return resources.Where(x =>
                            x.Resource.StatusId == (int)ResourceStatus.Active && x.LicenseType == LicenseType.Pda &&
                            !x.PdaAddedToCartDate.HasValue && !x.PdaDeletedDate.HasValue &&
                            x.PdaMaxViews > x.PdaViewCount)
                        .ToList();
                case PdaStatus.Deleted:
                    return resources.Where(x =>
                        x.Resource.StatusId == (int)ResourceStatus.Active && x.LicenseType == LicenseType.Pda &&
                        !x.PdaAddedToCartDate.HasValue && x.PdaDeletedDate.HasValue).ToList();
                case PdaStatus.Purchased:
                    return resources.Where(x =>
                        x.LicenseType == LicenseType.Purchased && x.PdaMaxViews == x.PdaViewCount &&
                        x.PdaAddedToCartDate.HasValue).ToList();
                case PdaStatus.NotPurchased:
                    return resources.Where(x =>
                        x.Resource.StatusId == (int)ResourceStatus.Active && x.LicenseType == LicenseType.Pda &&
                        x.PdaCartDeletedDate.HasValue).ToList();
                //case PdaStatus.None:
                default:
                    return resources;
            }
        }
    }
}