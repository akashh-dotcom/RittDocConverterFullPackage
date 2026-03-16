#region

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Core.Subscriptions;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Authentication
{
    public class AuthenticatedInstitutionService
    {
        private readonly IQueryable<InstitutionBranding> _institutionBrandings;
        private readonly IQueryable<InstitutionResourceLicense> _institutionResourceLicenses;
        private readonly IQueryable<Institution.Institution> _institutions;
        private readonly ILog<AuthenticatedInstitutionService> _log;
        private readonly IQueryable<ProductSubscription> _productSubscriptionspublisherUsers;
        private readonly IQueryable<ReserveShelf.ReserveShelf> _reserveShelves;
        private readonly IResourceService _resourceService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public AuthenticatedInstitutionService(ILog<AuthenticatedInstitutionService> log
            , IQueryable<ReserveShelf.ReserveShelf> reserveShelves
            , IQueryable<InstitutionBranding> institutionBrandings
            , IQueryable<ProductSubscription> productSubscriptionspublisherUsers
            , IQueryable<InstitutionResourceLicense> institutionResourceLicenses
            , IUnitOfWorkProvider unitOfWorkProvider
            , IQueryable<Institution.Institution> institutions
            , IResourceService resourceService
            , ISubscriptionService subscriptionService
        )
        {
            _log = log;
            _reserveShelves = reserveShelves;
            _institutionBrandings = institutionBrandings;
            _productSubscriptionspublisherUsers = productSubscriptionspublisherUsers;
            _institutionResourceLicenses = institutionResourceLicenses;
            _unitOfWorkProvider = unitOfWorkProvider;
            _institutions = institutions;
            _resourceService = resourceService;
            _subscriptionService = subscriptionService;
        }

        public AuthenticatedInstitution GetAuthenticatedInstitution(IUserWithFolders user, AuthenticationMethods method)
        {
            if (user.Role != null && (user.Role.Code == RoleCode.PUBUSER || user.Role.Code == RoleCode.SUBUSER))
            {
                var fakeInstitution = new Institution.Institution
                    { Id = int.MaxValue - 1, HomePageId = (int)HomePage.Titles, DisplayAllProducts = false };

                IList<InstitutionResourceLicense> licenses = new List<InstitutionResourceLicense>();

                if (user.Role.Code == RoleCode.SUBUSER)
                {
                    licenses = _subscriptionService.GetSubscriptionLicenses(user);

                    user.InstitutionId = fakeInstitution.Id;
                    var authenticatedInstitution =
                        new AuthenticatedInstitution(user, fakeInstitution, false, null, licenses, method);
                    return authenticatedInstitution;
                }
                else
                {
                    var publisherUser = user as PublisherUser;
                    if (publisherUser != null)
                    {
                        var allResources = _resourceService.GetAllResources()
                            .SkipWhile(x => x.PublisherId == 0 && x.Publisher == null).AsQueryable();

                        var resources = new List<IResource>();

                        if (publisherUser.Publisher.ConsolidatedPublisher == null)
                        {
                            var publisherResources =
                                allResources.Where(x => x.PublisherId == publisherUser.Publisher.Id);
                            var childPublisherResources = allResources.Where(x =>
                                x.Publisher.ConsolidatedPublisher != null && x.Publisher.ConsolidatedPublisher.Id ==
                                publisherUser.Publisher.Id);

                            resources.AddRange(publisherResources);
                            resources.AddRange(childPublisherResources);
                        }
                        else
                        {
                            var publisherResources =
                                allResources.Where(x => x.PublisherId == publisherUser.Publisher.Id);
                            var childPublisherResources = allResources.Where(x =>
                                x.Publisher.ConsolidatedPublisher != null && x.Publisher.ConsolidatedPublisher.Id ==
                                publisherUser.Publisher.Id);

                            var consolidatedPublisherResources = allResources.Where(x =>
                                x.PublisherId == publisherUser.Publisher.ConsolidatedPublisher.Id);
                            var consolidatedChildPublisherResources = allResources.Where(x =>
                                x.Publisher.ConsolidatedPublisher != null && x.Publisher.ConsolidatedPublisher.Id ==
                                publisherUser.Publisher.ConsolidatedPublisher.Id);

                            resources.AddRange(publisherResources);
                            resources.AddRange(childPublisherResources);
                            resources.AddRange(consolidatedPublisherResources);
                            resources.AddRange(consolidatedChildPublisherResources);
                        }


                        foreach (CachedResource resource in resources)
                        {
                            licenses.Add(new InstitutionResourceLicense
                            {
                                FirstPurchaseDate = publisherUser.Publisher.CreationDate,
                                Id = 0,
                                LicenseCount = 3,
                                ResourceId = resource.Id,
                                RecordStatus = true,
                                LicenseTypeId = (int)LicenseType.Trial
                            });
                        }
                    }

                    user.InstitutionId = fakeInstitution.Id;
                    var authenticatedInstitution = new AuthenticatedInstitution(publisherUser, fakeInstitution, false,
                        null, licenses, method);
                    return authenticatedInstitution;
                }
            }

            {
                var authenticatedInstitution =
                    BuildAuthenticatedInstitution(user.InstitutionId.GetValueOrDefault(), method, user);

                if (authenticatedInstitution != null &&
                    authenticatedInstitution.AccountStatus.Id == InstitutionAccountStatus.Trial.Id)
                {
                    var resources = _resourceService.GetAllResources();
                    _log.DebugFormat("calling AddTrialLicenses() - Id: {0}, account number: {1}",
                        authenticatedInstitution.Id, authenticatedInstitution.AccountNumber);
                    authenticatedInstitution.AddTrialLicenses(resources);
                }

                return authenticatedInstitution;
            }
        }

        /// <summary>
        /// </summary>
        public AuthenticatedInstitution GetAuthenticatedInstitution(int institutionId, AuthenticationMethods method)
        {
            return BuildAuthenticatedInstitution(institutionId, method, null);
        }

        /// <summary>
        ///     Queries the database to obtain the institition data and returns an AuthenticatedInstitution.
        ///     SJS - 10/14/2013 - Refactored to improve performance of login and ultimately minimize the amount of data returned
        ///     from the nhibernate queries. Using .ToFuture() was returning much more data than was needed.
        /// </summary>
        private AuthenticatedInstitution BuildAuthenticatedInstitution(int institutionId, AuthenticationMethods method,
            IUserWithFolders user)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var institution = _institutions.SingleOrDefault(i => i.Id == institutionId);
                if (institution == null)
                {
                    _log.WarnFormat("INSTITUTION IS NOT FOUND, institutionId: {0}", institutionId);
                    return null;
                }

                if (institution.AccountStatus == InstitutionAccountStatus.TrialExpired)
                {
                    _log.Info("Expired Trial!");
                    return null;
                }

                if (institution.AccountStatus == InstitutionAccountStatus.Disabled)
                {
                    _log.Info("Institution is DISABLED!");
                    return null;
                }

                var hasReserveShelf = _reserveShelves.Any(x => x.Institution.Id == institutionId);
                _log.DebugFormat("++ reserverShelves ++, hasReserveShelf: {0}", hasReserveShelf);

                var brandings = _institutionBrandings.SingleOrDefault(x => x.Institution.Id == institutionId);
                _log.Debug("++ reserverShelves ++");

                var institutionResourceLicenses =
                    _institutionResourceLicenses.Where(x => x.InstitutionId == institutionId).ToArray();

                _log.DebugFormat("++ institutionResourceLicenses ++, count: {0}", institutionResourceLicenses.Count());
                var productSubscriptionspublisherUsers =
                    _productSubscriptionspublisherUsers.Where(x => x.InstitutionId == institutionId).ToArray();
                _log.DebugFormat("++ productSubscriptionspublisherUsers ++, count: {0}",
                    productSubscriptionspublisherUsers.Count());

                stopwatch.Stop();
                _log.DebugFormat("BuildAuthenticatedInstitution(institutionId: {0}), load time: {1} ms", institutionId,
                    stopwatch.ElapsedMilliseconds);

                var authenticatedInstitution = user == null
                    ? new AuthenticatedInstitution(institution, hasReserveShelf, brandings,
                        institutionResourceLicenses, method)
                    : new AuthenticatedInstitution(user, institution, hasReserveShelf, brandings,
                        institutionResourceLicenses, method);
                return authenticatedInstitution;
            }
        }
    }
}