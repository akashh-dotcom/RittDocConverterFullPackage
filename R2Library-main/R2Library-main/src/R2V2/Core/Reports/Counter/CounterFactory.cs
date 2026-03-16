#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Transform;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Reports.Counter
{
    public class CounterFactory
    {
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public CounterFactory(IUnitOfWorkProvider unitOfWorkProvider)
        {
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        #region Previous Counter Version

        /// <summary>
        ///     Book #2
        /// </summary>
        public CounterSuccessfulResourcesRequest GetCounterSuccessfulSectionRequests(ReportRequest request)
        {
            request.Type = ReportType.CounterSectionRequests;
            IEnumerable<CounterSuccessfulResourceDataBase> successfulResourcesFromDataBase;

            var sql = GetCounterSectionRequestsQuery(request);

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                var results =
                    query.SetResultTransformer(Transformers.AliasToBean(typeof(CounterSuccessfulResourceDataBase)))
                        .List<CounterSuccessfulResourceDataBase>();

                successfulResourcesFromDataBase = results.ToList();
            }

            //_reportService.LogReportRequest(request);
            return new CounterSuccessfulResourcesRequest(request, successfulResourcesFromDataBase);
        }

        /// <summary>
        ///     Book #3
        /// </summary>
        public CounterTurnawayResourcesRequest GetCounterTurnawayResourceRequests(ReportRequest request)
        {
            request.Type = ReportType.CounterDeniedRequests;
            IEnumerable<CounterTurnawayResourceDataBase> counterTurnawayResourcesFromDataBase;

            var sql = GetCounterContentTurnawayQuery(request);

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                var results =
                    query.SetResultTransformer(Transformers.AliasToBean(typeof(CounterTurnawayResourceDataBase)))
                        .List<CounterTurnawayResourceDataBase>();

                counterTurnawayResourcesFromDataBase = results.ToList();
            }

            //_reportService.LogReportRequest(request);
            return new CounterTurnawayResourcesRequest(request, counterTurnawayResourcesFromDataBase);
        }

        //

        /// <summary>
        ///     Book #5
        /// </summary>
        public CounterSearchesRequest GetCounterSearchResourceRequests(ReportRequest request)
        {
            request.Type = ReportType.CounterSearchRequests;
            IEnumerable<CounterSuccessfulResourceDataBase> counterContentRequests;

            var sql = GetCounterTotalBookSearchesQuery(request);

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                var results =
                    query.SetResultTransformer(Transformers.AliasToBean(typeof(CounterSuccessfulResourceDataBase)))
                        .List<CounterSuccessfulResourceDataBase>();

                counterContentRequests = results.ToList();
            }

            //_reportService.LogReportRequest(request);
            return new CounterSearchesRequest(request, counterContentRequests);
        }

        /// <summary>
        ///     Platform #1
        /// </summary>
        public CounterTotalSearchesRequest GetCounterTotalSearchesRequests(ReportRequest request)
        {
            request.Type = ReportType.CounterPlatformRequests;
            var searchRequests = GetCounterPlatformSearches(request);
            var sectionRequests = GetCounterPlatformSearchClicks(request);
            var resourceRequests = GetCounterPlatformResourceViews(request);
            //_reportService.LogReportRequest(request);
            return new CounterTotalSearchesRequest(searchRequests, sectionRequests, resourceRequests, request);
        }


        /// <summary>
        ///     Platform #1
        ///     - GetCounterPlatformSearches
        /// </summary>
        private List<RequestPeriod> GetCounterPlatformSearches(ReportRequest request)
        {
            var sql = GetCounterPlatformSearchesQuery(request);

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                var results =
                    query.SetResultTransformer(Transformers.AliasToBean(typeof(RequestPeriod))).List<RequestPeriod>();

                return results.ToList();
            }
        }

        /// <summary>
        ///     Platform #1
        ///     - GetCounterPlatformSearchClicks
        /// </summary>
        private List<RequestPeriod> GetCounterPlatformSearchClicks(ReportRequest request)
        {
            var sql = GetCounterPlatformSearchClicksQuery(request);

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                var results =
                    query.SetResultTransformer(Transformers.AliasToBean(typeof(RequestPeriod))).List<RequestPeriod>();

                return results.ToList();
            }
        }

        /// <summary>
        ///     Platform #1
        ///     - GetCounterPlatformResourceViews
        /// </summary>
        private List<RequestPeriod> GetCounterPlatformResourceViews(ReportRequest request)
        {
            var sql = GetCounterPlatformResourceViewsQuery(request);

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                var results = query.SetResultTransformer(Transformers.AliasToBean(typeof(RequestPeriod)))
                    .List<RequestPeriod>();

                return results.ToList();
            }
        }


        #region "Counter Book #2 Query"

        /// <summary>
        ///     Book #2
        /// </summary>
        private static string GetCounterSectionRequestsQuery(ReportRequest request)
        {
            var sql = new StringBuilder()
                .Append(
                    " select r.vchResourceTitle as 'Title', p.vchPublisherName as 'Publisher', r.vchIsbn10 as 'Isbn10', r.vchIsbn13 as 'Isbn13', sum(dcvc.contentViewCount) as 'HitCount' ")
                .Append("     , Month(dcvc.contentViewDate) as 'Month', Year(dcvc.contentViewDate) as 'Year' ")
                .Append(" from vDailyContentViewCount dcvc ")
                .Append(" join tResource r on dcvc.resourceId = r.iResourceId ")
                .Append(" join tPublisher p on r.ipublisherId = p.iPublisherId ");
            if (request.IncludePurchasedTitles && !request.IncludePdaTitles)
            {
                sql.Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and dcvc.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseTypeId = 1 ");
            }
            else if (!request.IncludePurchasedTitles && request.IncludePdaTitles)
            {
                sql.Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and dcvc.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseTypeId = 3 ");
            }
            else
            {
                sql.Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and dcvc.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseTypeId in (1,3) ");
            }

            sql.Append(
                $" where dcvc.institutionId = {request.InstitutionId} and (dcvc.contentViewDate between '{request.DateRangeStart}' and '{request.DateRangeEnd}') and dcvc.chapterSectionId is not null ");

            //This is added to filter trial stats.
            if (request.IsTrialAccount)
            {
                //DO Nothing
            }
            else if (!request.IncludeTrialStats)
            {
                sql.Append(" and dcvc.licenseType <> 2 ");
            }
            //This is needed if you want to look at TRIAL stats only and they are an active/pda account.
            else if (request.IncludeTrialStats && !request.IncludePdaTitles && !request.IncludePurchasedTitles &&
                     !request.IncludeTocTitles)
            {
                sql.Append(" and dcvc.licenseType = 2 ");
            }

            sql.Append(
                    " group by Month(dcvc.contentViewDate), Year(dcvc.contentViewDate), r.vchResourceTitle, p.vchPublisherName ")
                .Append("     , r.vchIsbn10, r.vchIsbn13 ")
                .Append(" order by r.vchResourceTitle ");

            return sql.ToString();
        }

        #endregion

        #region "Counter Book #3 Query"

        /// <summary>
        ///     Book #3
        /// </summary>
        private static string GetCounterContentTurnawayQuery(ReportRequest request)
        {
            var sql = new StringBuilder()
                .Append(
                    " select r.vchResourceTitle as 'Title', p.vchPublisherName as 'Publisher', r.vchIsbn10 as 'Isbn10', r.vchIsbn13 as 'Isbn13', sum(v.contentTurnawayCount) as 'HitCount' ")
                .Append(
                    "     , v.turnawayTypeId as 'TurnawayTypeId', lv.vchLookupShortDesc as 'TurnawayType', Month(contentTurnawayDate) as 'Month', Year(contentTurnawayDate) as 'Year' ")
                .Append(" from vDailyContentTurnawayCount v ")
                .Append(" join tResource r on v.resourceId = r.iResourceId ")
                .Append(" join tPublisher p on r.ipublisherId = p.iPublisherId ")
                .Append(" join tLookupValues lv on v.turnawayTypeId = lv.iLookupValueId ");

            if (request.IncludePurchasedTitles && !request.IncludePdaTitles)
            {
                sql.Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and v.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseTypeId = 1 ");
            }
            else if (!request.IncludePurchasedTitles && request.IncludePdaTitles)
            {
                sql.Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and v.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseTypeId = 3 ");
            }
            else
            {
                sql.Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and v.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseTypeId in (1,3) ");
            }

            sql.Append(
                    $" where v.institutionId = {request.InstitutionId} and (v.contentTurnawayDate between '{request.DateRangeStart}' and '{request.DateRangeEnd}') ")
                .Append(
                    " group by Month(contentTurnawayDate), Year(contentTurnawayDate), r.vchResourceTitle, p.vchPublisherName ")
                .Append("     , v.turnawayTypeId, lv.vchLookupShortDesc, r.vchIsbn10, r.vchIsbn13 ")
                .Append(" order by r.vchResourceTitle ");

            return sql.ToString();
        }

        #endregion

        #region "Counter Book #5 Query"

        /// <summary>
        ///     Book #5
        /// </summary>
        private static string GetCounterTotalBookSearchesQuery(ReportRequest request)
        {
            var sql = new StringBuilder()
                .Append(
                    " select r.vchResourceTitle as 'Title', p.vchPublisherName as 'Publisher', r.vchIsbn10 as 'Isbn10', r.vchIsbn13 as 'Isbn13', sum(dcvc.contentViewCount) as 'HitCount' ")
                .Append("     , Month(dcvc.contentViewDate) as 'Month', Year(dcvc.contentViewDate) as 'Year' ")
                .Append(" from vDailyContentViewCount dcvc ")
                .Append(" join tResource r on dcvc.resourceId = r.iResourceId ")
                .Append(" join tPublisher p on r.ipublisherId = p.iPublisherId ");

            if (request.IncludePurchasedTitles && !request.IncludePdaTitles)
            {
                sql.Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and dcvc.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseTypeId = 1 ");
            }
            else if (!request.IncludePurchasedTitles && request.IncludePdaTitles)
            {
                sql.Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and dcvc.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseTypeId = 3 ");
            }
            else
            {
                sql.Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and dcvc.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseTypeId in (1,3) ");
            }

            sql.Append(
                $" where dcvc.institutionId = {request.InstitutionId} and (dcvc.contentViewDate between '{request.DateRangeStart}' and '{request.DateRangeEnd}') and dcvc.foundFromSearch = 1 ");
            //This is added to filter trial stats.
            if (request.IsTrialAccount)
            {
                //DO Nothing
            }
            else if (!request.IncludeTrialStats)
            {
                sql.Append(" and dcvc.licenseType <> 2 ");
            }
            //This is needed if you want to look at TRIAL stats only and they are an active/pda account.
            else if (request.IncludeTrialStats && !request.IncludePdaTitles && !request.IncludePurchasedTitles &&
                     !request.IncludeTocTitles)
            {
                sql.Append(" and dcvc.licenseType = 2 ");
            }

            sql.Append(
                    " group by Month(dcvc.contentViewDate), Year(dcvc.contentViewDate), r.vchResourceTitle, p.vchPublisherName ")
                .Append("     , r.vchIsbn10, r.vchIsbn13 ")
                .Append(" order by r.vchResourceTitle ");
            return sql.ToString();
        }

        #endregion

        #region "Counter Platform #1 Query"

        private static string GetCounterPlatformSearchesQuery(ReportRequest request)
        {
            var sql = new StringBuilder()
                .Append(" select sum(searchCount) as 'HitCount', month(searchDate) as 'Month' ")
                .Append("         , year(searchDate) as 'Year' ")
                .Append(" from vDailySearchCount ")
                .Append(" where ")
                .Append($" institutionId = {request.InstitutionId} and ")
                .Append($" (searchDate between '{request.DateRangeStart}' and '{request.DateRangeEnd}') ")
                .Append(" group by month(searchDate), year(searchDate) ")
                .Append(" order by year(searchDate), month(searchDate) ")
                .ToString();
            return sql;
        }

        private static string GetCounterPlatformSearchClicksQuery(ReportRequest request)
        {
            var sql = new StringBuilder()
                .Append(" select sum(dcvc.contentViewCount) as 'HitCount' ")
                .Append(" , month(dcvc.contentViewDate) as 'Month' , year(dcvc.contentViewDate) as 'Year' ")
                .Append(" from vDailyContentViewCount dcvc ");
            if (request.IncludePurchasedTitles && !request.IncludePdaTitles)
            {
                sql.Append("  join tResource r on dcvc.resourceId = r.iResourceId ");
                sql.Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and dcvc.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseTypeId = 1 ");
            }
            else if (!request.IncludePurchasedTitles && request.IncludePdaTitles)
            {
                sql.Append("  join tResource r on dcvc.resourceId = r.iResourceId ");
                sql.Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and dcvc.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseTypeId = 3 ");
            }
            else if (request.IncludePurchasedTitles && request.IncludePdaTitles)
            {
                sql.Append("  join tResource r on dcvc.resourceId = r.iResourceId ");
                sql.Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and dcvc.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseTypeId in (1,3) ");
            }

            //do not include licenses if neither license type is clicked
            sql.Append(" where ")
                .Append($" dcvc.institutionId = {request.InstitutionId} and ")
                .Append($" (dcvc.contentViewDate between '{request.DateRangeStart}' and '{request.DateRangeEnd}') ")
                .Append(" and dcvc.foundFromSearch = 1 ");
            //This is added to filter trial stats.
            if (request.IsTrialAccount)
            {
                //DO Nothing
            }
            else if (!request.IncludeTrialStats)
            {
                sql.Append(" and dcvc.licenseType <> 2 ");
            }
            //This is needed if you want to look at TRIAL stats only and they are an active/pda account.
            else if (request.IncludeTrialStats && !request.IncludePdaTitles && !request.IncludePurchasedTitles &&
                     !request.IncludeTocTitles)
            {
                sql.Append(" and dcvc.licenseType = 2 ");
            }

            sql.Append(" group by month(dcvc.contentViewDate), year(dcvc.contentViewDate) ")
                .Append(" order by year(dcvc.contentViewDate), month(dcvc.contentViewDate) ");
            return sql.ToString();
        }

        private static string GetCounterPlatformResourceViewsQuery(ReportRequest request)
        {
            var sql = new StringBuilder()
                .Append(
                    " select  sum(dcvc.contentViewCount) as 'HitCount', sum(dcvc.uniqueContentViewCount) as 'UniqueHitCount' ")
                .Append(" , month(dcvc.contentViewDate) as 'Month' , year(dcvc.contentViewDate) as 'Year' ")
                .Append(" from vDailyContentViewCount dcvc ");

            if (request.IncludePurchasedTitles && !request.IncludePdaTitles)
            {
                sql.Append("  join tResource r on dcvc.resourceId = r.iResourceId ");
                sql.Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and dcvc.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseTypeId = 1 ");
            }
            else if (!request.IncludePurchasedTitles && request.IncludePdaTitles)
            {
                sql.Append("  join tResource r on dcvc.resourceId = r.iResourceId ");
                sql.Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and dcvc.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseTypeId = 3 ");
            }
            else if (request.IncludePurchasedTitles && request.IncludePdaTitles)
            {
                sql.Append("  join tResource r on dcvc.resourceId = r.iResourceId ");
                sql.Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and dcvc.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseTypeId in (1,3) ");
            }

            //do not include licenses if neither license type is clicked
            sql.Append(" where ")
                .Append($" dcvc.institutionId = {request.InstitutionId} and ")
                .Append($" (dcvc.contentViewDate between '{request.DateRangeStart}' and '{request.DateRangeEnd}') ");
            //This is added to filter trial stats.
            if (request.IsTrialAccount)
            {
                //DO Nothing
            }
            else if (!request.IncludeTrialStats)
            {
                sql.Append(" and dcvc.licenseType <> 2 ");
            }
            //This is needed if you want to look at TRIAL stats only and they are an active/pda account.
            else if (request.IncludeTrialStats && !request.IncludePdaTitles && !request.IncludePurchasedTitles &&
                     !request.IncludeTocTitles)
            {
                sql.Append(" and dcvc.licenseType = 2 ");
            }

            sql.Append(" group by month(dcvc.contentViewDate), year(dcvc.contentViewDate) ")
                .Append(" order by year(dcvc.contentViewDate), month(dcvc.contentViewDate) ");
            return sql.ToString();
        }

        #endregion

        #endregion Previous Counter Version

        #region Counter 5.0

        #region Book Requests and Book Access Denied Requests

        /// <summary>
        ///     Book Requests
        /// </summary>
        public CounterBookRequests GetCounterBookRequests(ReportRequest request)
        {
            request.Type = ReportType.CounterBookRequests;
            IEnumerable<CounterBookRequestDataBase> bookRequestFromDataBase;

            var sql = GetCounterBookRequestsQuery(request);

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                var results =
                    query.SetResultTransformer(Transformers.AliasToBean(typeof(CounterBookRequestDataBase)))
                        .List<CounterBookRequestDataBase>();

                bookRequestFromDataBase = results.ToList();
            }

            //_reportService.LogReportRequest(request);
            return new CounterBookRequests(request, bookRequestFromDataBase);
        }

        /// <summary>
        ///     Book Access Denied Requests
        /// </summary>
        public CounterBookAccessDeniedRequests GetCounterBookAccessDeniedRequests(ReportRequest request)
        {
            request.Type = ReportType.CounterDeniedRequests;
            IEnumerable<CounterBookAccessDeniedResourceDataBase> counterBookAccessDeniedFromDataBase;

            var sql = GetCounterBookAccessDeniedRequestsQuery(request);

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                var results =
                    query.SetResultTransformer(
                            Transformers.AliasToBean(typeof(CounterBookAccessDeniedResourceDataBase)))
                        .List<CounterBookAccessDeniedResourceDataBase>();

                counterBookAccessDeniedFromDataBase = results.ToList();
            }

            //_reportService.LogReportRequest(request);
            return new CounterBookAccessDeniedRequests(request, counterBookAccessDeniedFromDataBase);
        }

        #endregion Book Requests and Book Access Denied Requests

        #region Platform Usage Requests

        /// <summary>
        ///     Platform Usage
        /// </summary>
        public CounterPlatformUsageRequest GetCounterPlatformUsageRequests(ReportRequest request)
        {
            request.Type = ReportType.CounterPlatformRequests;
            var totalItemRequests = GetCounterPlatformTotalItemRequests(request);
            var uniqueItemRequests = GetCounterPlatformUniqueItemRequests(request);
            var uniqueTitleRequests = GetCounterPlatformUniqueTitleRequests(request);
            //_reportService.LogReportRequest(request);
            return new CounterPlatformUsageRequest(totalItemRequests, uniqueItemRequests, uniqueTitleRequests, request);
        }

        /// <summary>
        ///     Platform Usage
        ///     - GetCounterPlatformTotalItemRequests
        /// </summary>
        private List<RequestPeriod> GetCounterPlatformTotalItemRequests(ReportRequest request)
        {
            var sql = GetCounterPlatformTotalItemRequestsQuery(request);

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                var results =
                    query.SetResultTransformer(Transformers.AliasToBean(typeof(RequestPeriod))).List<RequestPeriod>();

                return results.ToList();
            }
        }

        /// <summary>
        ///     Platform Usage
        ///     - GetCounterPlatformUniqueItemRequests
        /// </summary>
        private List<RequestPeriod> GetCounterPlatformUniqueItemRequests(ReportRequest request)
        {
            var sql = GetCounterPlatformUniqueItemRequestsQuery(request);

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                var results =
                    query.SetResultTransformer(Transformers.AliasToBean(typeof(RequestPeriod))).List<RequestPeriod>();

                return results.ToList();
            }
        }

        /// <summary>
        ///     Platform Usage
        ///     - GetCounterPlatformUniqueTitleRequests
        /// </summary>
        private List<RequestPeriod> GetCounterPlatformUniqueTitleRequests(ReportRequest request)
        {
            var sql = GetCounterPlatformUniqueTitleRequestsQuery(request);

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                var results = query.SetResultTransformer(Transformers.AliasToBean(typeof(RequestPeriod)))
                    .List<RequestPeriod>();

                return results.ToList();
            }
        }


        private static string GetCounterPlatformTotalItemRequestsQuery(ReportRequest request)
        {
            return GetCounter5PlatformContentQuery(request, true, false, false, false);
        }

        private static string GetCounterPlatformUniqueItemRequestsQuery(ReportRequest request)
        {
            return GetCounter5PlatformContentQuery(request, true, false, false, true);
        }

        private static string GetCounterPlatformUniqueTitleRequestsQuery(ReportRequest request)
        {
            return GetCounter5PlatformContentQuery(request, false, true, false, true);
        }

        #endregion Platform Usage

        #region Counter Book Requests Query

        private static string GetCounterBookRequestsQuery(ReportRequest request)
        {
            var result = new StringBuilder();

            var sqlItemRequests = GetCounterBookRequestsQuery(request, true, false, "Item");
            var sqlTitleRequests = GetCounterBookRequestsQuery(request, false, true, "Title");

            result.Append(sqlItemRequests)
                .Append(" union ")
                .Append(sqlTitleRequests)
                .Append(" order by r.vchResourceTitle, RequestType");

            return result.ToString();
        }


        private static string GetCounterBookRequestsQuery(ReportRequest request, bool includeChapterSectionContentOnly,
            bool includeTocContentOnly, string requestType)
        {
            var sql = new StringBuilder()
                .Append(
                    " select r.vchResourceTitle as 'Title', p.vchPublisherName as 'Publisher', 'R2Library:' + CAST(p.iPublisherId as varchar) as 'PublisherId' ")
                .Append("   , 'R2Library:' + r.vchIsbn13 as 'ProprietaryId'")
                .Append(
                    "   , r.vchIsbn10 as 'Isbn10', r.vchIsbn13 as 'Isbn13', sum(dcvc.contentViewCount) as 'HitCount' ")
                .Append($"  , sum(dcvc.uniqueContentViewCount) as 'UniqueHitCount', '{requestType}' as 'RequestType' ")
                .Append(
                    "   , Month(dcvc.contentViewDate) as 'Month', Year(dcvc.contentViewDate) as 'Year', Year(r.dtResourcePublicationDate) as 'YearOfPublication' ")
                .Append(" from vDailyContentViewCount dcvc ")
                .Append(" join tResource r on dcvc.resourceId = r.iResourceId ")
                .Append(" join tPublisher p on r.ipublisherId = p.iPublisherId ")
                .Append(
                    " left join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and dcvc.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 ")
                .Append(
                    $" where dcvc.institutionId = {request.InstitutionId} and (dcvc.contentViewDate between '{request.DateRangeStart}' and '{request.DateRangeEnd}') ");

            if (includeTocContentOnly)
            {
                sql.Append(" and dcvc.chapterSectionId is null ");
            }

            if (includeChapterSectionContentOnly)
            {
                sql.Append(" and dcvc.chapterSectionId is not null ");
            }

            var licenseTypeIDs = GetCounter5ContentQueryLicenseTypeIDs(request);
            if (!request.IncludeTrialStats)
            {
                sql.Append(" and dcvc.licenseType <> 2 ");

                if (!request.IncludePurchasedTitles || !request.IncludePdaTitles)
                {
                    sql.Append($" and irl.tiLicenseTypeId not in ({licenseTypeIDs}) ");
                }
                else
                {
                    sql.Append(" and irl.iInstitutionResourceLicenseId is not null ");
                }
            }
            else if (!request.IncludePurchasedTitles || !request.IncludePdaTitles)
            {
                sql.Append(
                    $" and (irl.iInstitutionResourceLicenseId is null or irl.tiLicenseTypeId not in ({licenseTypeIDs}))");
            }

            sql.Append(
                    " group by Month(dcvc.contentViewDate), Year(dcvc.contentViewDate), r.vchResourceTitle, p.vchPublisherName, p.iPublisherId ")
                .Append("     , r.vchIsbn10, r.vchIsbn13, r.dtResourcePublicationDate ");
            //.Append(" order by r.vchResourceTitle ");

            return sql.ToString();
        }

        #endregion

        #region Counter Book Access Denied Query

        private static string GetCounterBookAccessDeniedRequestsQuery(ReportRequest request)
        {
            var sql = new StringBuilder()
                .Append(
                    " select r.vchResourceTitle as 'Title', p.vchPublisherName as 'Publisher', 'R2Library:' + CAST(p.iPublisherId as varchar) as 'PublisherId' ")
                .Append("   , 'R2Library:' + r.vchIsbn13 as 'ProprietaryId' ")
                .Append(
                    "   , r.vchIsbn10 as 'Isbn10', r.vchIsbn13 as 'Isbn13', sum(v.contentTurnawayCount) as 'HitCount' ")
                .Append(
                    "   , v.turnawayTypeId as 'TurnawayTypeId', lv.vchLookupShortDesc as 'TurnawayType', Month(contentTurnawayDate) as 'Month', Year(contentTurnawayDate) as 'Year' ")
                .Append("   , Year(r.dtResourcePublicationDate) as 'YearOfPublication'")
                .Append(" from vDailyContentTurnawayCount v ")
                .Append(" join tResource r on v.resourceId = r.iResourceId ")
                .Append(" join tPublisher p on r.ipublisherId = p.iPublisherId ")
                .Append(" join tLookupValues lv on v.turnawayTypeId = lv.iLookupValueId ")
                .Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and v.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 ")
                .Append(
                    $" where v.institutionId = {request.InstitutionId} and (v.contentTurnawayDate between '{request.DateRangeStart}' and '{request.DateRangeEnd}') ");

            if (!request.IncludePurchasedTitles || !request.IncludePdaTitles)
            {
                var licenseTypeIDs = GetCounter5ContentQueryLicenseTypeIDs(request);
                sql.Append($" and irl.tiLicenseTypeId not in ({licenseTypeIDs}) ");
            }

            sql.Append(
                    " group by Month(contentTurnawayDate), Year(contentTurnawayDate), r.vchResourceTitle, p.vchPublisherName, p.iPublisherId ")
                .Append(
                    "     , v.turnawayTypeId, lv.vchLookupShortDesc, r.vchIsbn10, r.vchIsbn13, r.dtResourcePublicationDate ")
                .Append(" order by r.vchResourceTitle ");

            return sql.ToString();
        }

        #endregion

        #region Content Query

        private static string GetCounter5PlatformContentQuery(ReportRequest request,
            bool includeChapterSectionContentOnly, bool includeTocContentOnly, bool includeTurnaways,
            bool includeUniquesOnly)
        {
            var uniquePrefix = includeUniquesOnly ? "unique" : "";

            var sql = new StringBuilder()
                .Append($" select sum(dcvc.{uniquePrefix}ContentViewCount) as 'HitCount' ")
                .Append(" , month(dcvc.contentViewDate) as 'Month' , year(dcvc.contentViewDate) as 'Year' ")
                .Append(" from vDailyContentViewCount dcvc ")
                .Append("  join tResource r on dcvc.resourceId = r.iResourceId ")
                .Append(
                    " left join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and dcvc.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 ")
                .Append(" where ")
                .Append($" dcvc.institutionId = {request.InstitutionId} and ")
                .Append($" (dcvc.contentViewDate between '{request.DateRangeStart}' and '{request.DateRangeEnd}') ");

            if (includeTocContentOnly)
            {
                sql.Append(" and dcvc.chapterSectionId is null ");
            }

            if (includeChapterSectionContentOnly)
            {
                sql.Append(" and dcvc.chapterSectionId is not null ");
            }

            var licenseTypeIDs = GetCounter5ContentQueryLicenseTypeIDs(request);
            if (!request.IncludeTrialStats)
            {
                sql.Append(" and dcvc.licenseType <> 2 ");

                if (!request.IncludePurchasedTitles || !request.IncludePdaTitles)
                {
                    sql.Append($" and irl.tiLicenseTypeId not in ({licenseTypeIDs}) ");
                }
                else
                {
                    sql.Append(" and irl.iInstitutionResourceLicenseId is not null ");
                }
            }
            else if (!request.IncludePurchasedTitles || !request.IncludePdaTitles)
            {
                sql.Append(
                    $" and (irl.iInstitutionResourceLicenseId is null or irl.tiLicenseTypeId not in ({licenseTypeIDs}))");
            }

            sql.Append(" group by month(dcvc.contentViewDate), year(dcvc.contentViewDate) ")
                .Append(" order by year(dcvc.contentViewDate), month(dcvc.contentViewDate) ");

            if (includeTurnaways)
            {
                sql.Append(" union all ");

                var turnawaysQuery = GetCounter5ContentTurnawaysQuery(request);
                sql.Append(turnawaysQuery);
            }

            return sql.ToString();
        }

        private static string GetCounter5ContentTurnawaysQuery(ReportRequest request,
            bool includeChapterSectionContentOnly = false, bool includeTocContentOnly = false)
        {
            var sql = new StringBuilder()
                .Append(" select sum(dctc.contentTurnawayCount) as 'HitCount' ")
                .Append(" , month(dctc.contentTurnawayDate) as 'Month' , year(dctc.contentTurnawayDate) as 'Year' ")
                .Append(" from vDailyContentTurnawayCount dctc ")
                .Append("  join tResource r on dctc.resourceId = r.iResourceId ")
                .Append(
                    " join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and dctc.institutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 ")
                .Append(" where ")
                .Append($" dctc.institutionId = {request.InstitutionId} and ")
                .Append($" (dctc.contentViewDate between '{request.DateRangeStart}' and '{request.DateRangeEnd}') ");

            if (includeTocContentOnly)
            {
                sql.Append(" and dctc.chapterSectionId is null ");
            }

            if (includeChapterSectionContentOnly)
            {
                sql.Append(" and dctc.chapterSectionId is not null ");
            }

            if (!request.IncludePurchasedTitles || !request.IncludePdaTitles)
            {
                var licenseTypeIDs = GetCounter5ContentQueryLicenseTypeIDs(request);
                sql.Append($" and irl.tiLicenseTypeId not in ({licenseTypeIDs}) ");
            }

            sql.Append(" group by month(dctc.contentTurnawayDate), year(dctc.contentTurnawayDate) ")
                .Append(" order by year(dctc.contentTurnawayDate), month(dctc.contentTurnawayDate) ");

            return sql.ToString();
        }

        private static string GetCounter5ContentQueryLicenseTypeIDs(ReportRequest request)
        {
            var licenseTypeIDsList = new List<int>();
            if (!request.IncludePurchasedTitles)
            {
                licenseTypeIDsList.Add(1);
            }

            if (!request.IncludePdaTitles)
            {
                licenseTypeIDsList.Add(3);
            }

            return string.Join(",", licenseTypeIDsList);
        }

        #endregion Content Query

        #endregion Counter 5.0
    }
}