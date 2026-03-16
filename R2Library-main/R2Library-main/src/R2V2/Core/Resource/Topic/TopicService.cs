#region

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Resource.Topic
{
    public class TopicService
    {
        private const string AlphaKeysAllKey = "AlphaKeys.All.Active";
        private const string AtoZIndexKeyPrefix = "Topics.Alpha.";
        private const string ResourceTopicCacheKey = "Resource.Topic.Cache";

        private static readonly object AlphaKeyLockObject = new object();
        private static readonly object AtoZIndexLockObject = new object();
        private readonly IApplicationWideStorageService _applicationWideStorageService;
        private readonly IQueryable<AZIndex> _azIndex;

        private readonly ILog<TopicService> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public TopicService(
            ILog<TopicService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IQueryable<AZIndex> azIndex
            , IApplicationWideStorageService applicationWideStorageService
        )
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _azIndex = azIndex;
            _applicationWideStorageService = applicationWideStorageService;
        }

        public List<string> GetAllAlphaKeys()
        {
            var allAlphaKeys = _applicationWideStorageService.Get<List<string>>(AlphaKeysAllKey);
            if (allAlphaKeys == null)
            {
                lock (AlphaKeyLockObject)
                {
                    allAlphaKeys = _applicationWideStorageService.Get<List<string>>(AlphaKeysAllKey);
                    if (allAlphaKeys == null)
                    {
                        _log.Debug("loading alpha keys");
                        using (var uow = _unitOfWorkProvider.Start())
                        {
                            allAlphaKeys = _azIndex.Select(x => x.AlphaKey).Distinct().ToList();

                            _log.DebugFormat("alpha key count: {0}", allAlphaKeys.Count());
                            foreach (var key in allAlphaKeys)
                            {
                                uow.Evict(key);
                            }

                            _applicationWideStorageService.Put(AlphaKeysAllKey, allAlphaKeys);
                        }
                    }
                }
            }

            return allAlphaKeys;
        }

        public List<AZIndex> GetAtoZIndexForKey(string alphaKey)
        {
            var applicationKey = $"{AtoZIndexKeyPrefix}{alphaKey}";

            var azIndices = _applicationWideStorageService.Get<List<AZIndex>>(applicationKey);
            if (azIndices == null)
            {
                lock (AtoZIndexLockObject)
                {
                    azIndices = _applicationWideStorageService.Get<List<AZIndex>>(applicationKey);
                    if (azIndices == null)
                    {
                        _log.DebugFormat("loading a-z index terms for {0}", alphaKey);
                        using (var uow = _unitOfWorkProvider.Start())
                        {
                            var query = _azIndex.Where(x => x.AlphaKey == alphaKey).OrderBy(x => x.Name);

                            azIndices = query.ToList();

                            _log.DebugFormat("a-z index terms: {0}", azIndices.Count());
                            foreach (var azIndex in azIndices)
                            {
                                uow.Evict(azIndex);
                            }

                            _applicationWideStorageService.Put(applicationKey, azIndices);
                        }
                    }
                }
            }

            return azIndices;
        }

        /// <summary>
        ///     This method will run a SQL select statement to get the topics from the database
        ///     SJS - 4/23/2013
        ///     This method was written to improve the performance of the alpha index.  Using link/nhibernate
        ///     the query typically took between 800 and 1500 milliseconds to run.  The statement below run between 400 and 800
        ///     milliseconds.
        ///     If someone can get that same performance using nhibernate, feel free to change this.
        /// </summary>
        public IEnumerable<string> GetTopics(int institutionId, string alphaKey, bool displayAllProducts,
            int practiceAreaId, int specialtyId, AtoZIndexType atoZIndexType)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //select distinct az.vchName
            //from   tAtoZIndex az
            // join  tResource r on az.iResourceId = r.iResourceId and r.tiRecordStatus = 1
            // join  tInstitutionResourceLicense irl on az.iResourceId = irl.iResourceId and irl.tiRecordStatus = 1
            // join  dbo.tResourcePracticeArea rpa on rpa.iResourceId = r.iResourceId and rpa.iPracticeAreaId = 1
            // join  dbo.tResourceSpecialty rs on rs.iResourceId = r.iResourceId and rs.iSpecialtyId = 10
            //where  az.chrAlphaKey = 'C'
            //  and ((irl.iInstitutionId = 1 and (irl.tiLicenseTypeId = 1 and irl.iLicenseCount > 0)
            //                                or (irl.tiLicenseTypeId = 3 and irl.dtPdaAddedToCartDate is null))
            //    or (1 = 1 and r.NotSaleable = 0))
            //order by az.vchName

            // abscess, cancer, Acetaminophen, Xylosum
            var sql = new StringBuilder()
                .Append("select distinct az.vchName ")
                .Append("from   dbo.tAtoZIndex az ")
                .Append(" join  dbo.tResource r on az.iResourceId = r.iResourceId and r.tiRecordStatus = 1 ")
                .Append(
                    " join  dbo.tInstitutionResourceLicense irl on az.iResourceId = irl.iResourceId and irl.tiRecordStatus = 1 ");

            if (practiceAreaId > 0)
            {
                // sjs - 10/16/2015 - added check to make sure the tResourcePracticeArea record has not been soft deleted
                sql.Append(
                    " join  dbo.tResourcePracticeArea rpa on rpa.iResourceId = r.iResourceId and rpa.iPracticeAreaId = :practiceAreaId and rpa.tiRecordStatus = 1 ");
            }

            if (specialtyId > 0)
            {
                sql.Append(
                    " join  dbo.tResourceSpecialty rs on rs.iResourceId = r.iResourceId and rs.iSpecialtyId = :specialtyId ");
            }

            sql.Append("where  az.chrAlphaKey = :alphaKey ")
                .Append(
                    "  and ((irl.iInstitutionId = :institutionId and (irl.tiLicenseTypeId = 1 and irl.iLicenseCount > 0) ")
                .Append(
                    "                                or (irl.tiLicenseTypeId = 3 and irl.dtPdaAddedToCartDate is null)) ")
                .Append("    or (1 = :displayAllProducts and r.NotSaleable = 0)) ")
                .AppendFormat("  and  az.iAtoZIndexTypeId in ({0}) ", GetAtoZIndexTypeIds(atoZIndexType))
                .Append("order by az.vchName;");

            //_log.Debug(sql.ToString());

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql.ToString());

                query.SetParameter("alphaKey", alphaKey);
                query.SetParameter("institutionId", institutionId);
                query.SetParameter("displayAllProducts", displayAllProducts ? 1 : 0);

                if (practiceAreaId > 0)
                {
                    query.SetParameter("practiceAreaId", practiceAreaId);
                }

                if (specialtyId > 0)
                {
                    query.SetParameter("specialtyId", specialtyId);
                }

                var results = query.List<string>();

                stopwatch.Stop();
                _log.DebugFormat("topics count: {0}, retrieved in {1} ms", results.Count,
                    stopwatch.ElapsedMilliseconds);

                var debugMsg = new StringBuilder();
                foreach (var result in results)
                {
                    debugMsg.Append(result).Append(", ");
                }

                _log.Debug(debugMsg.ToString());

                return results;
            }
        }

        private string GetAtoZIndexTypeIds(AtoZIndexType atoZIndexType)
        {
            switch (atoZIndexType)
            {
                case AtoZIndexType.All:
                    return $"{(int)AtoZIndexType.Disease},{(int)AtoZIndexType.Drug},{(int)AtoZIndexType.Keyword}";

                case AtoZIndexType.Disease:
                    return $"{(int)AtoZIndexType.Disease}";

                case AtoZIndexType.DiseaseSynonym:
                    return $"{(int)AtoZIndexType.DiseaseSynonym}";

                case AtoZIndexType.Drug:
                    return $"{(int)AtoZIndexType.Drug}";

                case AtoZIndexType.DrugSynonym:
                    return $"{(int)AtoZIndexType.DrugSynonym}";

                case AtoZIndexType.Keyword:
                    return $"{(int)AtoZIndexType.Keyword}";
            }

            return $"{(int)AtoZIndexType.Disease},{(int)AtoZIndexType.Drug},{(int)AtoZIndexType.Keyword}";
        }

        public IEnumerable<string> GetResourceTopics(string isbn)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //return _azIndex
            //    .Where(x => x.Isbn == isbn && x.Name != null && x.Name.Length > 0)
            //    .OrderBy(x => x.Name)
            //    .Select(x => x.Name)
            //    .Distinct()
            //    .ToList();
            var resourceTopicCache = GetResourceTopicCache(isbn);
            var topics = resourceTopicCache.Topics.Where(x => !string.IsNullOrEmpty(x.Topic))
                .Select(topic => topic.Topic).Distinct().ToList();

            stopwatch.Stop();
            _log.Debug(
                $"GetResourceTopics(isbn: {isbn}) - {stopwatch.ElapsedMilliseconds:##,##0}ms - topics.Count: {topics.Count:##,##0}");
            return topics;
        }

        public IEnumerable<string> GetResourceTopics(string isbn, string sectionId)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var chapterId = !string.IsNullOrEmpty(sectionId) && sectionId.Length > 7
                ? sectionId.Substring(0, 6)
                : sectionId;

            //return _azIndex
            //    .Where(x => x.Isbn == isbn && (x.ChapterId == chapterId || x.SectionId == sectionId))
            //    .Select(x => x.Name)
            //    .Distinct()
            //    .ToList();

            var resourceTopicCache = GetResourceTopicCache(isbn);
            var topics = resourceTopicCache.Topics.Where(x => x.Chapter == chapterId || x.Section == sectionId)
                .Select(topic => topic.Topic).Distinct().ToList();

            stopwatch.Stop();
            _log.Debug(
                $"GetResourceTopics(isbn: {isbn}, sectionId: {sectionId}) - {stopwatch.ElapsedMilliseconds:##,##0}ms - topics.Count: {topics.Count:##,##0}");
            return topics;
        }

        private ResourceTopicCache GetResourceTopicCache(string isbn)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            ResourceTopicCache resourceTopicCache;

            ResourceTopics resourceTopics;
            if (!_applicationWideStorageService.Has(ResourceTopicCacheKey))
            {
                resourceTopics = new ResourceTopics();
                _applicationWideStorageService.Put(ResourceTopicCacheKey, resourceTopics);
            }
            else
            {
                resourceTopics = _applicationWideStorageService.Get<ResourceTopics>(ResourceTopicCacheKey);
            }

            if (resourceTopics.Resources.ContainsKey(isbn))
            {
                resourceTopicCache = resourceTopics.Resources[isbn];
                stopwatch.Stop();
                _log.Debug(
                    $"GetResourceTopicCache(isbn: {isbn}) - {stopwatch.ElapsedMilliseconds:##,##0}ms - resourceTopicCache.Topics.Count: {resourceTopicCache.Topics.Count:##,##0} - cache hit");
                return resourceTopicCache;
            }

            resourceTopicCache = new ResourceTopicCache { Isbn = isbn };
            resourceTopics.Resources.Add(isbn, resourceTopicCache);

            var atozIndex =
                from a in _azIndex
                where a.Isbn == isbn
                orderby a.Name
                select new
                {
                    a.ResourceId, a.Isbn, a.Name, a.ChapterId,
                    a.SectionId
                };

            foreach (var aToZ in atozIndex)
            {
                var topicCache = new TopicCache
                {
                    Topic = aToZ.Name,
                    Chapter = aToZ.ChapterId,
                    Section = aToZ.SectionId
                };
                resourceTopicCache.ResourceId = aToZ.ResourceId;
                resourceTopicCache.Topics.Add(topicCache);
            }

            stopwatch.Stop();
            _log.Debug(
                $"GetResourceTopicCache(isbn: {isbn}) - {stopwatch.ElapsedMilliseconds:##,##0}ms - resourceTopicCache.Topics.Count: {resourceTopicCache.Topics.Count:##,##0} - new");
            return resourceTopicCache;
        }
    }
}