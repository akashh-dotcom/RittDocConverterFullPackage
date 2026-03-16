#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NHibernate;
using NHibernate.Linq;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Publisher;
using R2V2.Core.Reports;
using R2V2.Core.Resource.BookSearch;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Resource
{
    public class ResourceService : IResourceService
    {
        private const string ResourcesPublicationCacheKey = "Resources.PublicationYears";
        private readonly IApplicationWideStorageService _applicationWideStorageService;
        private readonly IContentSettings _contentSettings;
        private readonly DashboardService _dashboardService;
        private readonly FeaturedTitleService _featuredTitleService;

        private readonly ILog<ResourceService> _log;
        private readonly ResourceCacheService _resourceCacheService;
        private readonly IQueryable<Resource> _resources;
        private readonly IQueryable<IResource> _resources2;
        private readonly SpecialDiscountResourceFactory _specialDiscountResourceFactory;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public ResourceService(
            ILog<ResourceService> log
            , IQueryable<Resource> resources
            , IQueryable<IResource> resources2
            , IApplicationWideStorageService applicationWideStorageService
            , IContentSettings contentSettings
            , IUnitOfWorkProvider unitOfWorkProvider
            , ResourceCacheService resourceCacheService
            , FeaturedTitleService featuredTitleService
            , SpecialDiscountResourceFactory specialDiscountResourceFactory
            , DashboardService dashboardService
        )
        {
            _log = log;
            _resources = resources;
            _resources2 = resources2;
            _applicationWideStorageService = applicationWideStorageService;
            _contentSettings = contentSettings;
            _unitOfWorkProvider = unitOfWorkProvider;
            _resourceCacheService = resourceCacheService;
            _featuredTitleService = featuredTitleService;
            _specialDiscountResourceFactory = specialDiscountResourceFactory;
            _dashboardService = dashboardService;
        }


        public void ReloadResourceCache()
        {
            _resourceCacheService.GetResourceCache(true, true);
        }

        public IResource GetResource(int resourceId)
        {
            var methodTimer = new Stopwatch();
            methodTimer.Start();

            var resourceCache = GetResourceCache(false);
            var resource = resourceCache.GetResourceById(resourceId);
            SetResourceImageUrl(resource);

            methodTimer.Stop();
            if (methodTimer.ElapsedMilliseconds > 50)
            {
                _log.WarnFormat("GetResource(resourceId: {0}), method time: {1} ms", resourceId,
                    methodTimer.ElapsedMilliseconds);
            }

            return resource;
        }

        public Resource GetResourceForEdit(int resourceId)
        {
            var methodTimer = new Stopwatch();
            methodTimer.Start();

            var resource = _resources.SingleOrDefault(x => x.Id == resourceId);
            SetResourceImageUrl(resource);

            methodTimer.Stop();
            _log.DebugFormat("GetResourceForEdit(resourceId: {0}), method time: {1} ms", resourceId,
                methodTimer.ElapsedMilliseconds);
            return resource;
        }

        public IList<Resource> GetResources(IEnumerable<int> resourceIds)
        {
            var methodTimer = new Stopwatch();
            methodTimer.Start();

            var resources = _resources.Where(r => resourceIds.Contains(r.Id)).ToList();
            //SetResourceImageUrl(resource);

            methodTimer.Stop();
            _log.DebugFormat("GetResources(), resourceIds.Count(): {0} method time: {1} ms, resources.Count: {2}",
                resourceIds.Count(), methodTimer.ElapsedMilliseconds, resources.Count);
            return resources;
        }

        public IList<IResource> GetResourcesForOngoingPda(string[] resourceIsbns)
        {
            var methodTimer = new Stopwatch();
            methodTimer.Start();

            // Determine which session to use
            ISession sessionToUse;
            var isNewSession = false;

            if (_unitOfWorkProvider.Session != null && _unitOfWorkProvider.Session.IsOpen)
            {
                sessionToUse = _unitOfWorkProvider.Session;
                _log.Debug("Using existing open session");
            }
            else
            {
                sessionToUse = _unitOfWorkProvider.Session.SessionFactory.OpenSession();
                isNewSession = true;
                _log.Debug("Opened new session because existing session was null or closed");
            }

            try
            {
                var resources = sessionToUse.Query<IResource>()
                    .Where(x => resourceIsbns.Contains(x.Isbn))
                    .ToList();

                methodTimer.Stop();
                _log.DebugFormat(
                    "GetResourcesForOngoingPda(), resourceIsbns.Count(): {0}, method time: {1} ms, resources.Count: {2}, new session: {3}",
                    resourceIsbns.Length, methodTimer.ElapsedMilliseconds, resources.Count, isNewSession);

                return resources;
            }
            finally
            {
                // Dispose the new session if we created one
                if (isNewSession)
                {
                    sessionToUse.Dispose();
                    _log.Debug("Disposed new session");
                }
            }
        }

        public Resource GetSoftDeletedResource(int resourceId)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            using (var uow = _unitOfWorkProvider.Start())
            {
                uow.IncludeSoftDeletedValues();
                var resource = _resources.SingleOrDefault(x => x.Id == resourceId);
                SetResourceImageUrl(resource);

                stopwatch.Stop();
                _log.DebugFormat("GetSoftDeletedResource(resourceId: {0}), method time: {1} ms", resourceId,
                    stopwatch.ElapsedMilliseconds);
                _log.Debug(resource != null ? resource.ToDebugInfo() : "resource NOT FOUND!");
                uow.ExcludeSoftDeletedValues();
                return resource;
            }
        }

        public List<IResource> GetDuplicateIsbns(string isbn10, string isbn13, string eIsbn, int resourceId)
        {
            var allResources = GetAllResources();

            if (!string.IsNullOrWhiteSpace(isbn10))
            {
                if (!string.IsNullOrWhiteSpace(isbn13))
                {
                    if (!string.IsNullOrWhiteSpace(eIsbn))
                    {
                        //All 3 ISBNs <> null
                        return allResources.Where(x => x.Id != resourceId && (
                            x.Isbn10 == isbn10 ||
                            x.Isbn == isbn13 || x.Isbn13 == isbn13 || x.EIsbn == isbn13 ||
                            x.Isbn == eIsbn || x.Isbn13 == eIsbn || x.EIsbn == eIsbn
                        )).ToList();
                    }

                    //eISBN == null
                    return allResources.Where(x => x.Id != resourceId && (
                        x.Isbn10 == isbn10 ||
                        x.Isbn == isbn13 || x.Isbn13 == isbn13 || x.EIsbn == isbn13
                    )).ToList();
                }

                //eISBN == null && isbn13
                return allResources.Where(x => x.Id != resourceId && x.Isbn10 == isbn10).ToList();
            }

            if (!string.IsNullOrWhiteSpace(isbn13))
            {
                if (!string.IsNullOrWhiteSpace(eIsbn))
                {
                    //isbn10 == null
                    return allResources.Where(x => x.Id != resourceId && (
                        x.Isbn == isbn13 || x.Isbn13 == isbn13 || x.EIsbn == isbn13 ||
                        x.Isbn == eIsbn || x.Isbn13 == eIsbn || x.EIsbn == eIsbn
                    )).ToList();
                }

                //isbn10 == null && eisbn == null
                return allResources.Where(x => x.Id != resourceId && (
                    x.Isbn == isbn13 || x.Isbn13 == isbn13 || x.EIsbn == isbn13
                )).ToList();
            }

            if (!string.IsNullOrWhiteSpace(eIsbn))
            {
                return allResources.Where(x => x.Id != resourceId && (
                    x.Isbn == eIsbn || x.Isbn13 == eIsbn || x.EIsbn == eIsbn
                )).ToList();
            }

            //All ISBNs are null
            // This should never happen but returning all should stop what ever is validate.
            return allResources.ToList();
        }

        public IResource GetResource(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return null;
            }

            IResource resource;

            try
            {
                resource = GetAllResources().SingleOrDefault(r =>
                    r.Isbn == isbn || r.Isbn10 == isbn || r.Isbn13 == isbn || r.EIsbn == isbn);

                if (resource != null)
                {
                    SetResourceImageUrl(resource);
                    return resource;
                }

                // SJS - 9/12/2012 - If resource is not found, query data to see if the resource has been loaded since the cache was created
                resource = _resources.SingleOrDefault(r =>
                    r.Isbn == isbn || r.Isbn10 == isbn || r.Isbn13 == isbn || r.EIsbn == isbn);
                if (resource != null)
                {
                    _log.DebugFormat("GetResource(isbn: {0}) - force resource cache reload", isbn);
                    resource = GetAllResources(true).SingleOrDefault(r =>
                        r.Isbn == isbn || r.Isbn10 == isbn || r.Isbn13 == isbn || r.EIsbn == isbn);
                }
                else
                {
                    _log.InfoFormat("GetResource(isbn: {0}) - resource not found", isbn);
                }

                SetResourceImageUrl(resource);
                return resource;
            }
            catch (InvalidOperationException exception)
            {
                _log.Error("Duplicate resource: " + isbn, exception);

                resource = GetAllResources().FirstOrDefault(x => x.Isbn == isbn);
            }

            SetResourceImageUrl(resource);
            return resource;
        }

        public List<IResource> GetRecentlyReleasedTitles(int resourceId, int specialtyId, bool isDci)
        {
            var resourcesToReturn = new List<IResource>();

            try
            {
                var resources = GetAllResources()
                    .Where(x => (x.StatusId == (int)ResourceStatus.Active ||
                                 x.StatusId == (int)ResourceStatus.Forthcoming) &&
                                x.Id != resourceId).ToList();

                if (specialtyId > 0)
                {
                    resources = resources.Where(x => x.Specialties.Any(y => y.Id == specialtyId)).ToList();
                }

                if (resources.Count > 5)
                {
                    if (isDci)
                    {
                        //Collection.Id 5 = DCT  Collection.Id 24 = Doody Reviewed
                        var doodySpecificTitles = resources.Where(x => x.Collections.Any(y => y.Id == 24 || y.Id == 5))
                            .ToList();
                        resourcesToReturn.AddRange(doodySpecificTitles.OrderByDescending(x => x.ReleaseDate).Take(5));
                    }
                    else
                    {
                        resourcesToReturn.AddRange(resources.OrderByDescending(x => x.ReleaseDate).Take(5));
                    }

                    var currentCount = resourcesToReturn.Count;
                    var resourcesRemaining = resources.Where(x => !resourcesToReturn.Contains(x));

                    resourcesToReturn.AddRange(resourcesRemaining.OrderByDescending(x => x.ReleaseDate)
                        .Take(5 - currentCount));
                }
                else
                {
                    resourcesToReturn.AddRange(resources);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return resourcesToReturn;
        }

        public IResource GetResourceWithoutDatabase(string isbn)
        {
            try
            {
                var resource = GetAllResources().SingleOrDefault(r =>
                    r.Isbn == isbn || r.Isbn10 == isbn || r.Isbn13 == isbn || r.EIsbn == isbn);

                if (resource != null)
                {
                    SetResourceImageUrl(resource);
                    return resource;
                }

                return null;
            }
            catch (InvalidOperationException exception)
            {
                _log.Error("Duplicate resource: " + isbn, exception);
                return null;
            }
        }

        public IEnumerable<IResource> GetAllResources()
        {
            return GetAllResources(false);
        }

        public IEnumerable<IResource> GetAllResources(bool forceReload)
        {
            var resourceCache = GetResourceCache(forceReload);
            return resourceCache.GetAllResources();
        }

        public IEnumerable<IResource> GetResources(IResourceQuery resourceQuery, IPublisher featuredPublisher,
            IEnumerable<IFeaturedTitle> allFeaturedTitles, IList<IPublisher> publishers)
        {
            return GetResources(resourceQuery, featuredPublisher, allFeaturedTitles, false, null, publishers);
        }

        public IEnumerable<IResource> GetResources(IResourceQuery resourceQuery, IPublisher featuredPublisher,
            IEnumerable<IFeaturedTitle> allFeaturedTitles, bool includeFutureSpecials, int[] recentResourceIds,
            IList<IPublisher> publishers)
        {
            var query = !string.IsNullOrWhiteSpace(resourceQuery.Query) ? resourceQuery.Query.ToUpper().Trim() : "";

            var filteredResources = GetFilteredResources(query, publishers);

            return FilterResources(filteredResources, resourceQuery, featuredPublisher, allFeaturedTitles,
                includeFutureSpecials, recentResourceIds);
        }


        public IEnumerable<IResource> GetResources(IEnumerable<IResource> filteredResources,
            IResourceQuery resourceQuery, IPublisher featuredPublisher, IEnumerable<IFeaturedTitle> allFeaturedTitles)
        {
            return FilterResources(filteredResources, resourceQuery, featuredPublisher, allFeaturedTitles, false, null);
        }

        public IEnumerable<IResource> GetResources(IEnumerable<IResource> filteredResources,
            IResourceQuery resourceQuery, IPublisher featuredPublisher, IEnumerable<IFeaturedTitle> allFeaturedTitles,
            bool includeFutureSpecials, int[] recentResourceIds)
        {
            return FilterResources(filteredResources, resourceQuery, featuredPublisher, allFeaturedTitles,
                includeFutureSpecials, recentResourceIds);
        }


        public IEnumerable<IResource> GetResourcesExcludeIds(IResourceQuery resourceQuery, int[] resourceIds,
            IList<IPublisher> publishers)
        {
            var query = !string.IsNullOrWhiteSpace(resourceQuery.Query) ? resourceQuery.Query.ToUpper().Trim() : "";

            var filteredResources = GetFilteredResources(query, publishers);

            filteredResources = filteredResources.Where(x => !resourceIds.Contains(x.Id));

            var specialDiscountResources = _specialDiscountResourceFactory.GetSpecialResourcesDiscount();

            return filteredResources
                .FilterBy(resourceQuery.ResourceStatus)
                .FilterBy(resourceQuery.ResourceFilterType, specialDiscountResources)
                .PracticeAreaFilterBy(resourceQuery.PracticeAreaFilter)
                .SpecialtyFilterBy(resourceQuery.SpecialtyFilter)
                .CollectionFilterBy(resourceQuery.CollectionFilter)
                .CollectionListFilterBy(resourceQuery.CollectionListFilter)
                .OrderBy(resourceQuery);
        }

        public IEnumerable<IResource> GetResourcesByIds(int[] resourceIds)
        {
            var allResources = GetAllResources();

            return allResources.Where(x => resourceIds.Contains(x.Id));
        }

        public IList<IResource> GetResources(string[] isbns)
        {
            var methodTimer = new Stopwatch();
            methodTimer.Start();
            var resources = GetAllResources().ToList();
            var filteredResources = resources.Where(x => !string.IsNullOrWhiteSpace(x.Isbn) && isbns.Contains(x.Isbn))
                .ToList();
            //var test = _resources.Where(x => isbns.Contains(x.Isbn));
            //List<Resource> resources  = test.ToList();
            //methodTimer.Stop();
            _log.DebugFormat("GetResources(), isbns.Count(): {0} method time: {1} ms, resources.Count: {2}",
                isbns.Count(), methodTimer.ElapsedMilliseconds, filteredResources.Count);
            return filteredResources;
        }

        public string ConvertIsbn10To13(string isbn10)
        {
            if (!IsValidateIsbn(isbn10))
            {
                _log.WarnFormat("Invalid ISBN 10: {0}", isbn10);
                return null;
            }

            var cleanedIsbn10 = isbn10.Replace("-", string.Empty).Replace(" ", string.Empty);

            if (cleanedIsbn10.Length == 10)
            {
                var isbn13 = $"978{isbn10.Substring(0, 9)}";
                var a = Convert.ToInt32(isbn13.Substring(0, 1));
                var b = Convert.ToInt32(Convert.ToInt32(isbn13.Substring(1, 1)) * 3);
                var c = Convert.ToInt32(isbn13.Substring(2, 1));
                var d = Convert.ToInt32(Convert.ToInt32(isbn13.Substring(3, 1)) * 3);
                var e = Convert.ToInt32(isbn13.Substring(4, 1));
                var f = Convert.ToInt32(Convert.ToInt32(isbn13.Substring(5, 1)) * 3);
                var g = Convert.ToInt32(isbn13.Substring(6, 1));
                var h = Convert.ToInt32(Convert.ToInt32(isbn13.Substring(7, 1)) * 3);
                var i = Convert.ToInt32(isbn13.Substring(8, 1));
                var j = Convert.ToInt32(Convert.ToInt32(isbn13.Substring(9, 1)) * 3);
                var k = Convert.ToInt32(isbn13.Substring(10, 1));
                var l = Convert.ToInt32(Convert.ToInt32(isbn13.Substring(11, 1)) * 3);
                var sum = a + b + c + d + e + f + g + h + i + j + k + l;
                var checkdigit = 10 - sum % 10;
                if (checkdigit == 10)
                {
                    checkdigit = 0;
                }

                isbn13 = $"{isbn13}{checkdigit}";

                if (!IsValidateIsbn(isbn13))
                {
                    _log.WarnFormat("Invalid ISBN 13: {0}", isbn13);
                    return null;
                }

                return isbn13;
            }

            _log.WarnFormat("Invalid ISBN 10: '{0}'", isbn10);
            return null;
        }


        /// <summary>
        /// Logic that will validate the isbn, making sure that it conforms to the internationally defined standard format of an ISBN.  Use RegEx for this.
        ///// http://www.regexlib.com/(A(bX0gu7eaQW4XW0EGUdb7rjoZUMePQd8wxI8H-I3GMYIs8QUzwX0HClMdYohnr-kBTOSggRsTTpJk30y5LOe83sUZJIPvVsoPAWZkyKYJsrgAVIRnlObcaTbleCHV7ACZneurNzosxRQ-_eVhefimhZMt4grc47-RlE79dkWjgGEcT6ZrEF2C2cVP-m4Ugbaj0))/Search.aspx?k=isbn&c=-1&m=-1&ps=20
        //  Expression = ^(97(8|9))?\d{9}(\d|X)$
        //  How to use REGEX with C# - http://www.c-sharpcorner.com/UploadFile/prasad_1/RegExpPSD12062005021717AM/RegExpPSD.aspx
        /// </summary>
        /// <returns>bool</returns>
        public bool IsValidateIsbn(string isbn)
        {
            if (string.IsNullOrEmpty(isbn))
            {
                return false;
            }

            //what's the @ sign for?  to escape the \ character.  http://stackoverflow.com/questions/1558058/bad-compile-constant-value
            var isbnRegex = new Regex(@"^(97(8|9))?\d{9}(\d|X)$");
            return isbnRegex.IsMatch(isbn);
        }

        public void ValidateAllResourceIsbns()
        {
            var allResources = GetAllResources();

            var errorMessage = new StringBuilder();
            errorMessage.AppendLine("INVALID RESOURCE ISBNs:");
            var invalidCount = 0;
            foreach (var resource in allResources)
            {
                if (resource.StatusId == (int)ResourceStatus.Inactive)
                {
                    continue;
                }

                //979 isbns are valid, but will fail the isbn10 check and cannot be converted to isbn10
                if (resource.Isbn.StartsWith("979"))
                {
                    continue;
                }

                if (!IsValidateIsbn(resource.Isbn10))
                {
                    errorMessage.AppendFormat(
                        "\tINVALID ISBN 10 - id: {0}, isbn: {1}, isbn 10: {2}, isbn 13: {3}, e-isbn: {4}, status id: {5}",
                        resource.Id, resource.Isbn, resource.Isbn10, resource.Isbn13, resource.EIsbn,
                        resource.StatusId);
                    errorMessage.AppendLine();
                    invalidCount++;
                }
                else
                {
                    var isbn13 = ConvertIsbn10To13(resource.Isbn10);
                    if (string.IsNullOrWhiteSpace(isbn13) || isbn13.Length != 13 || isbn13 != resource.Isbn13)
                    {
                        errorMessage.AppendFormat(
                            "\tINVALID ISBN 13 - {0} - id: {1}, isbn: {2}, isbn 10: {3}, isbn 13: {4}, e-isbn: {5}, status id: {6}",
                            isbn13, resource.Id, resource.Isbn, resource.Isbn10, resource.Isbn13, resource.EIsbn,
                            resource.StatusId);
                        errorMessage.AppendLine();
                        invalidCount++;
                    }
                }
            }

            if (invalidCount > 0)
            {
                _log.Error(errorMessage.ToString());
            }
        }

        public int[] GetResourcePublicationYears()
        {
            var years = _applicationWideStorageService.Get<int[]>(ResourcesPublicationCacheKey);
            if (years == null)
            {
                var resources = GetAllResources();

                var yearsList = new List<int>();
                foreach (var resource in resources)
                {
                    if (!resource.PublicationDate.HasValue)
                    {
                        continue;
                    }

                    var year = resource.PublicationDate.Value.Year;
                    if (!yearsList.Contains(year))
                    {
                        yearsList.Add(year);
                    }
                }

                years = yearsList.OrderByDescending(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();
                _applicationWideStorageService.Put(ResourcesPublicationCacheKey, years);
            }

            return years;
        }

        public void SaveResource(Resource resource)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.SaveOrUpdate(resource);
                    //uow.Merge(resource); // Merge seems to Stop the "Illegal attempt to associate a collection with two open sessions"
                    uow.Commit();
                    transaction.Commit();

                    uow.Evict(resource); // Needs to be evicted from the current session so the cache properly reloads it
                    // TODO:  Instead of evicting the resource here, we should probably just trigger a cache reload of this individual resource,
                    // TODO:  and not the brute force method of reloading the entire cache in the controller thats calling this method.
                }
            }
        }

        /// <summary>
        ///     Call a stored procedure to insert a record in the TransformQueue table in the R2Utilites database (STG_R2Utilities
        ///     on staging)
        ///     That is why we are use a stored proc.
        /// </summary>
        public void AddToTransformQueue(IResource resource)
        {
            _log.DebugFormat("Id: {0}, resource.Isbn: {1}, RecordStatus: {2}, StatusId: {3}", resource.Id,
                resource.Isbn, resource.RecordStatus, resource.StatusId);
            var acceptedStatus = new[]
                { (int)ResourceStatus.Active, (int)ResourceStatus.Archived, (int)ResourceStatus.Forthcoming };
            if (!resource.RecordStatus || !acceptedStatus.Contains(resource.StatusId))
            {
                return;
            }

            var query = _unitOfWorkProvider.Session.CreateSQLQuery(
                "exec sp_R2UtilitiesTransformQueueInsert @ResourceId = :ResourceId, @Isbn = :Isbn, @Status = :Status");
            query.SetParameter("ResourceId", resource.Id);
            query.SetParameter("Isbn", resource.Isbn);
            query.SetParameter("Status", 'A');

            var results = query.ExecuteUpdate();
            _log.DebugFormat("results: {0}", results);
        }

        public void UpdateAdminSearchFile(IResource resource)
        {
            var bookSearchResource = new BookSearchResource(resource, _contentSettings.UtilitiesContentLocation);
            bookSearchResource.SaveR2BookSearchXml();

            var query = _unitOfWorkProvider.Session.CreateSQLQuery(
                "exec sp_R2UtilitiesIndexQueueInsert @ResourceId = :ResourceId, @Isbn = :Isbn, @Status = :Status");
            query.SetParameter("ResourceId", resource.Id);
            query.SetParameter("Isbn", resource.Isbn);
            query.SetParameter("Status", 'A');

            var results = query.ExecuteUpdate();
            _log.DebugFormat("results: {0}", results);
        }

        public void AddToTransformQueue(IEnumerable<IResource> resources)
        {
            var counter = 0;
            var insertCounter = 0;

            var sb = new StringBuilder();
            foreach (var resource in resources)
            {
                counter++;
                sb.AppendFormat(
                    "exec sp_R2UtilitiesTransformQueueInsert @ResourceId = {0}, @Isbn = '{1}', @Status = 'A';",
                    resource.Id, resource.Isbn);
                if (counter > 50)
                {
                    var query = _unitOfWorkProvider.Session.CreateSQLQuery(sb.ToString());
                    insertCounter += query.ExecuteUpdate();
                    sb = new StringBuilder();
                }
            }

            if (!string.IsNullOrWhiteSpace(sb.ToString()))
            {
                var query = _unitOfWorkProvider.Session.CreateSQLQuery(sb.ToString());
                insertCounter += query.ExecuteUpdate();
            }

            _log.InfoFormat("Consolidated Publisher -- {0} Resources Added to TransformQueue", insertCounter);
        }

        public void DeleteResource(Resource resource)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.Delete(resource);
                    uow.Commit();
                    transaction.Commit();
                }
            }
        }

        public string GetCitation(IResource resource, string link)
        {
            const string separator = "; ";

            var citation = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(resource.Authors))
            {
                citation.Append(resource.Authors);
            }

            citation.Append(separator);

            citation.AppendFormat("{0}{1}", resource.Title, separator);

            if (!string.IsNullOrWhiteSpace(resource.Edition))
            {
                citation.Append(resource.Edition);
            }

            citation.Append(separator);

            if (!string.IsNullOrWhiteSpace(resource.Isbn))
            {
                citation.Append(resource.Isbn);
            }

            citation.Append(separator);

            if (!string.IsNullOrWhiteSpace(resource.Publisher.Name))
            {
                citation.Append(resource.Publisher.Name);
            }

            citation.Append(separator);

            if (resource.PublicationDate.HasValue)
            {
                citation.AppendFormat(resource.PublicationDate.Value.ToString("MM/dd/yyyy"));
            }

            citation.Append(separator);

            citation.AppendFormat("R2 OnLine Library. {0}", link);

            return citation.ToString();
        }

        public string GetProciteCitation(IResource resource, string url)
        {
            var sb = new StringBuilder();

            sb.Append("TY - BOOK").AppendLine();
            foreach (var author in resource.AuthorList)
            {
                var lastName = author.LastName;
                var firstName = $"{(!string.IsNullOrWhiteSpace(lastName) ? ", " : "")}{author.FirstName}";
                var lineage = $"{(!string.IsNullOrWhiteSpace(author.Lineage) ? ", " : "")}{author.Lineage}";
                if (!string.IsNullOrWhiteSpace(lastName))
                {
                    sb.AppendFormat("A1 - {0}{1}{2}", lastName, firstName, lineage).AppendLine();
                }
            }

            sb.AppendFormat("T1 - {0}", resource.Title.Replace('–', '-')).AppendLine()
                .AppendFormat("E1 - {0}", resource.Edition).AppendLine()
                .AppendFormat("PB - {0}", resource.Publisher.Name).AppendLine()
                .AppendFormat("Y1 - {0}/{1}/{2}/", resource.PublicationDate.GetValueOrDefault().Year,
                    resource.PublicationDate.GetValueOrDefault().Month,
                    resource.PublicationDate.GetValueOrDefault().Day).AppendLine()
                .AppendFormat("SN - {0}", resource.Isbn10).AppendLine()
                .AppendFormat("LK - {0}", url).AppendLine()
                .Append("ER -").AppendLine();
            return sb.ToString();
        }

        public string GetEndNoteCitation(IResource resource, string url)
        {
            var sb = new StringBuilder();

            sb.Append("%0 Electronic Book").AppendLine();

            foreach (var author in resource.AuthorList)
            {
                var lastName = author.LastName;
                var firstName = $"{(!string.IsNullOrWhiteSpace(lastName) ? ", " : "")}{author.FirstName}";
                if (!string.IsNullOrWhiteSpace(firstName))
                {
                    sb.AppendFormat("%A {0}{1}", lastName, firstName).AppendLine();
                }
            }

            sb.AppendFormat("%T {0}", resource.Title.Replace('–', '-')).AppendLine()
                .AppendFormat("%7 {0}", resource.Edition).AppendLine()
                .AppendFormat("%C {0}, {1}", resource.Publisher.City, resource.Publisher.State).AppendLine()
                .AppendFormat("%I {0}", resource.Publisher.Name).AppendLine()
                .AppendFormat("%D {0}", resource.PublicationDate.GetValueOrDefault().Year).AppendLine()
                .AppendFormat("%8 {0}", DateTime.Now.ToShortDateString()).AppendLine()
                .AppendFormat("%@ {0}", resource.Isbn10).AppendLine()
                .AppendFormat("%U {0}", url).AppendLine()
                .AppendLine();

            return sb.ToString();
        }

        public string GetRefWorksCitation(IResource resource, string url)
        {
            var sb = new StringBuilder();

            sb.Append("RT Book, Chapter").AppendLine();

            foreach (var author in resource.AuthorList)
            {
                var lastName = author.LastName;
                var firstName = $"{(!string.IsNullOrWhiteSpace(lastName) ? ", " : "")}{author.FirstName}";
                var lineage = $"{(!string.IsNullOrWhiteSpace(author.Lineage) ? ", " : "")}{author.Lineage}";
                if (!string.IsNullOrWhiteSpace(lastName))
                {
                    sb.AppendFormat("A1 {0}{1}{2}", lastName, firstName, lineage).AppendLine();
                }
            }

            sb.AppendFormat("T1 {0}", resource.Title.Replace('–', '-')).AppendLine();
            sb.AppendFormat("ED {0}", resource.Edition).AppendLine();
            sb.AppendFormat("PB {0}", resource.Publisher.Name).AppendLine();
            sb.AppendFormat("FD {0}", resource.PublicationDate.GetValueOrDefault().ToShortDateString()).AppendLine();
            sb.AppendFormat("SN {0}", resource.Isbn10).AppendLine();
            sb.AppendFormat("RD {0}", DateTime.Now.ToShortDateString()).AppendLine();
            sb.AppendFormat("PP {0}, {1}", resource.Publisher.City, resource.Publisher.State).AppendLine();
            sb.AppendFormat("LK {0}", url).AppendLine();

            return sb.ToString();
        }

        public string GetApaFormatCitation(IResource resource, string url)
        {
            return GetApaFormatCitationBase(resource, url, null);
        }

        public string GetApaFormatCitation(IResource resource, string url, string subTitle)
        {
            return GetApaFormatCitationBase(resource, url, subTitle);
        }

        /// <summary>
        ///     /
        /// </summary>
        private ResourceCache GetResourceCache(bool forceReload)
        {
            var resourceCache = _resourceCacheService.GetResourceCache(forceReload, forceReload);
            _featuredTitleService.GetFeaturedTitles(forceReload, resourceCache);
            return resourceCache;
        }

        private IEnumerable<IResource> FilterResources(IEnumerable<IResource> filteredResources,
            IResourceQuery resourceQuery, IPublisher featuredPublisher, IEnumerable<IFeaturedTitle> allFeaturedTitles,
            bool includeFutureSpecials, int[] recentResourceIds)
        {
            var specialDiscountResources = includeFutureSpecials
                ? _specialDiscountResourceFactory.GetSpecialResourcesDiscountForAdminResource()
                : _specialDiscountResourceFactory.GetSpecialResourcesDiscount();

            specialDiscountResources = specialDiscountResources ?? new List<CachedSpecialResource>();

            filteredResources = filteredResources
                .FilterBy(resourceQuery.ResourceStatus)
                .FilterBy(resourceQuery.ResourceFilterType, specialDiscountResources)
                .FilterBy(resourceQuery.PublisherId)
                .PracticeAreaFilterBy(resourceQuery.PracticeAreaFilter)
                .SpecialtyFilterBy(resourceQuery.SpecialtyFilter)
                .CollectionFilterBy(resourceQuery.CollectionFilter)

                #region "DO NOT INCLUDE Archived Resources for all Special Collections"

                .SpecialDiscountResourcesFilterBy(resourceQuery.IncludeSpecialDiscounts, specialDiscountResources)
                .CollectionListFilterBy(resourceQuery.CollectionListFilter)
                //.ClinicalCornerstoneFilterBy(resourceQuery.IncludeClinicalCornerstone)
                //.NoteworthyNursingFilterBy(resourceQuery.IncludeNoteworthyNursing)
                //.BestOfYearFilterBy(resourceQuery.IncludeBestOfYear)
                //.AjnBooks2015FilterBy(resourceQuery.IncludeAjnBooks2015)
                //.NursingEssentialsFilterBy(resourceQuery.IncludeNursingEssentials)
                //.HospitalEssentialsFilterBy(resourceQuery.IncludeHospitalEssentials)
                //.MedicalEssentialsFilterBy(resourceQuery.IncludeMedicalEssentials)
                .FreeResourcesFilterBy(resourceQuery.IncludeFreeResources)

                #endregion

                .RecentResourcesFilterBy(recentResourceIds, resourceQuery.RecentOnly)
                .OrderBy(resourceQuery)
                .OrderByRecent(resourceQuery, recentResourceIds);

            var collectionManagementQuery = resourceQuery as ICollectionManagementQuery;
            if (collectionManagementQuery != null)
            {
                switch (collectionManagementQuery.ResourceListType)
                {
                    case ResourceListType.FeaturedPublisher:
                        if (featuredPublisher != null)
                        {
                            filteredResources = filteredResources.Where(x => x.PublisherId == featuredPublisher.Id);
                        }

                        break;
                    case ResourceListType.FeaturedTitles:
                        var featuredTitles =
                            allFeaturedTitles.Where(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now)
                                .ToList();
                        filteredResources = featuredTitles.Any()
                            ? filteredResources.Where(x => featuredTitles.Select(ft => ft.ResourceId).Contains(x.Id))
                            : null;
                        break;
                    case ResourceListType.Purchased:
                    case ResourceListType.Archived:
                    case ResourceListType.NewEditionPurchased:
                    case ResourceListType.PdaAdded:
                    case ResourceListType.PdaAddedToCart:
                    case ResourceListType.PdaNewEdition:
                        if (collectionManagementQuery.DateRangeStart != DateTime.MinValue)
                        {
                            var resourceIds =
                                _dashboardService.GetFilteredResourceIds(collectionManagementQuery.ResourceListType,
                                    collectionManagementQuery.InstitutionId,
                                    collectionManagementQuery.DateRangeStart,
                                    collectionManagementQuery.DateRangeEnd
                                );
                            filteredResources = resourceIds.Any()
                                ? filteredResources.Where(x => resourceIds.Select(id => id).Contains(x.Id))
                                : null;
                        }

                        break;
                }
            }

            return filteredResources;
        }

        private IEnumerable<IResource> GetFilteredResources(string query, IList<IPublisher> publishers)
        {
            var allResources = GetAllResources();
            var filteredResources = new List<IResource>();

            var publisherIdsToFilterBy = GetPublisherIdsToFilterBy(query, publishers);

            foreach (var resource in allResources)
            {
                try
                {
                    // SJS - 8/23/2012 - all of the conditions are broken out into individual checks because
                    // we are having an issue with this code sometime throwing an exception.  Have all of the
                    // conditions in a single if makes it difficult to identify the issue. So don't this I'm
                    // an idiot for coding like this.

                    // check ISBNs
                    if (resource.Isbn10 == query || resource.Isbn13 == query || resource.EIsbn == query ||
                        resource.Isbn == query)
                    {
                        filteredResources.Add(resource);
                        continue;
                    }

                    // check title
                    if (resource.Title == null)
                    {
                        _log.ErrorFormat("resource.Title is null, resource.Id: {0}", resource.Id);
                    }
                    else
                    {
                        if (resource.Title.ToUpper().Contains(query))
                        {
                            filteredResources.Add(resource);
                            continue;
                        }
                    }

                    // check authors
                    if (resource.Authors == null)
                    {
                        _log.ErrorFormat("resource.Authors is null, resource.Id: {0}", resource.Id);
                    }
                    else
                    {
                        if (resource.Authors.ToUpper().Contains(query))
                        {
                            filteredResources.Add(resource);
                            continue;
                        }
                    }

                    // check publishers
                    if (resource.Publisher == null)
                    {
                        _log.ErrorFormat("resource.Publisher is null, resource.Id: {0}", resource.Id);
                    }
                    else if (resource.Publisher.Name == null)
                    {
                        _log.ErrorFormat("resource.Publisher.Name is null, resource.Id: {0}", resource.Id);
                    }
                    else if (publisherIdsToFilterBy != null && publisherIdsToFilterBy.Contains(resource.Publisher.Id))
                    {
                        filteredResources.Add(resource);
                        continue;
                    }

                    // check publication dates
                    if (resource.PublicationDate != null && resource.PublicationDate.ToString().Contains(query))
                    {
                        filteredResources.Add(resource);
                        continue;
                    }

                    // check copyright
                    if (resource.Copyright != null && resource.Copyright.Contains(query))
                    {
                        filteredResources.Add(resource);
                    }
                }
                catch (Exception ex)
                {
                    var msg = new StringBuilder();
                    msg.AppendFormat("Problem with Resource Id: {0}", resource.Id).AppendLine()
                        .Append(ex.Message);
                    _log.Error(msg.ToString(), ex);
                }
            }

            _log.DebugFormat("filteredResources: {0}", filteredResources.Count());
            return filteredResources;
        }

        /// <summary>
        ///     Returns an array of publisher Ids the resources will match if it is a Publisher query.
        ///     This is needed so you get all resources that match the publisher and consolidated publishers
        /// </summary>
        private int[] GetPublisherIdsToFilterBy(string query, IList<IPublisher> publishers)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return null;
            }

            var foundPublishers = publishers.Where(x => x.Name.ToUpper().Contains(query));

            if (foundPublishers.Any())
            {
                var foundPublishersList = publishers.Where(x => x.Name.ToUpper().Contains(query)).ToList();
                var associatedPublishers = foundPublishersList.Where(x => x.ConsolidatedPublisher != null)
                    .Select(x => x.ConsolidatedPublisher);
                if (associatedPublishers.Any())
                {
                    foundPublishersList.AddRange(associatedPublishers.ToList());
                }

                return foundPublishersList.Select(x => x.Id).ToArray();
            }

            return null;
        }

        private string GetApaFormatCitationBase(IResource resource, string url, string sectionTitle)
        {
            var sb = new StringBuilder();
            var authorCount = resource.AuthorList.Count();
            var currentAuthorCount = 0;
            foreach (var author in resource.AuthorList)
            {
                currentAuthorCount++;

                if (!string.IsNullOrWhiteSpace(author.LastName))
                {
                    sb.Append(author.LastName);
                }

                if (!string.IsNullOrWhiteSpace(author.FirstName))
                {
                    sb.AppendFormat("{0}{1}. ", !string.IsNullOrWhiteSpace(author.LastName) ? ", " : "",
                        author.FirstName.Substring(0, 1));
                }

                if (authorCount != currentAuthorCount)
                {
                    sb.Append("& ");
                }
            }

            sb.AppendFormat("({0}). ", resource.PublicationDate.GetValueOrDefault().Year);
            sb.AppendFormat("{0}{1}", string.IsNullOrWhiteSpace(sectionTitle) ? "" : $"{sectionTitle}. ",
                resource.Title.Replace('–', '-'));
            if (!string.IsNullOrWhiteSpace(resource.Edition))
            {
                sb.Append($" ({resource.Edition} ed.)");
            }

            sb.Append(". ");
            sb.Append($"{resource.Publisher.Name.TrimEnd()}. ");

            //sb.AppendFormat("Retrieved {0} {1}, {2} from ", DateTime.Now.ToString("MMMM"), DateTime.Now.Day, DateTime.Now.Year);
            sb.Append(url);

            return sb.ToString();
        }

        private void SetResourceImageUrl(IResource resource)
        {
            if (resource != null)
            {
                resource.ImageUrl = resource.ImageFileName.ToImageUrl(_contentSettings);
            }
        }
    }
}