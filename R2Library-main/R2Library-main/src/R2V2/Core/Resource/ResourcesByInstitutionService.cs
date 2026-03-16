#region

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using R2V2.Contexts;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Core.Resource
{
    public class ResourcesByInstitutionService : IResourcesByInstitutionService
    {
        private readonly IAuthenticationContext _authenticationContext;
        private readonly ILog<ResourcesByInstitutionService> _log;
        private readonly IResourceAccessService _resourceAccessService;

        private readonly IResourceService _resourceService;
        //private readonly ICollectionService _collectionService;

        public ResourcesByInstitutionService(
            ILog<ResourcesByInstitutionService> log
            , IAuthenticationContext authenticationContext
            , IResourceService resourceService
            , IResourceAccessService resourceAccessService
            //, ICollectionService collectionService
        )
        {
            _log = log;
            _authenticationContext = authenticationContext;
            _resourceService = resourceService;
            _resourceAccessService = resourceAccessService;
            //_collectionService = collectionService;
        }

        public IList<IResource> GetResourcesForActiveInstitution(string guestAccountNumber, bool tocAvailable,
            int collectionId = 0)
        {
            // BUG - DON'T USE IN CLAUSE WITH MULTIPLE QUERIES - MultiQuery/ToFuture broken with Contains (in) - https://nhibernate.jira.com/browse/NH-2897
            var stopwatch = new Stopwatch();

            var resourcesAll = _resourceService.GetAllResources();
            if (collectionId > 0)
            {
                resourcesAll = resourcesAll.Where(r => r.CollectionIdsToArray().Contains(collectionId));
            }

            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;

            IList<IResource> resources;
            if (!_authenticationContext.IsAuthenticated || authenticatedInstitution == null)
            {
                // public access
                resources = resourcesAll.Where(r =>
                        (r.StatusId == (int)ResourceStatus.Active || r.StatusId == (int)ResourceStatus.Archived) &&
                        !r.NotSaleable)
                    .ToList();
            }
            else
            {
                var displayAllProducts = true;
                if (authenticatedInstitution.AccountNumber != guestAccountNumber)
                {
                    displayAllProducts = authenticatedInstitution.DisplayAllProducts && tocAvailable;
                }

                resources = new List<IResource>();
                if (displayAllProducts)
                {
                    foreach (var resource in resourcesAll)
                    {
                        // the business rules when display all products is selected
                        // 1. Only show active and archived titles
                        // 2. Show if title is saleable
                        // 3. If not saleable, include only if the institution has a license for the title (IsFullTextAvailable)
                        if (resource.StatusId != (int)ResourceStatus.Active &&
                            resource.StatusId != (int)ResourceStatus.Archived)
                        {
                            continue;
                        }

                        if (!resource.NotSaleable)
                        {
                            resources.Add(resource);
                        }
                        else
                        {
                            // this is coded like this because for performance reasons.
                            // the license lookup is only performed when it is absolutely neccessary.
                            var license = authenticatedInstitution.GetResourceLicense(resource.Id);
                            if (license != null && _resourceAccessService.IsFullTextAvailable(license))
                            {
                                resources.Add(resource);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var resource in resourcesAll)
                    {
                        // SJS - 2/3/2014 - #477 – confirm that forthcoming titles do not show except in admin
                        if (resource.StatusId != (int)ResourceStatus.Active &&
                            resource.StatusId != (int)ResourceStatus.Archived)
                        {
                            continue;
                        }

                        var license = authenticatedInstitution.GetResourceLicense(resource.Id);
                        if (license != null && _resourceAccessService.IsFullTextAvailable(license))
                        {
                            resources.Add(resource);
                        }
                    }
                }
            }

            stopwatch.Stop();
            _log.DebugFormat("GetResourcesForInstitution() time: {0} ms, resources.Count: {1}",
                stopwatch.ElapsedMilliseconds, resources.Count);
            return resources;
        }
    }
}