#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Transform;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Core.Search;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Reports
{
    /// <summary>
    ///     This service class contains all of the database queries used by reports.
    ///     My apologies to anyone who needs to modify these queries - SJS - 7/31/2012
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IQueryable<IInstitution> _institutions;
        private readonly ILog<ReportService> _log;
        private readonly IQueryable<SavedReport> _savedReports;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IQueryable<User> _users;

        public ReportService(ILog<ReportService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IQueryable<SavedReport> savedReports
            , IQueryable<IInstitution> institutions
            , IQueryable<User> users
        )
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _savedReports = savedReports;
            _institutions = institutions;
            _users = users;
        }

        public ApplicationReportCounts GetApplicationReportCounts(ReportRequest reportRequest)
        {
            reportRequest.Type = ReportType.ApplicationUsageReport;
            var counts = new ApplicationReportCounts();
            SetDailyCounts(counts, reportRequest);
            _log.Debug(counts.ToString());
            LogReportRequest(reportRequest);
            return counts;
        }

        public List<ResourceReportItem> GetResourceReportItems(ReportRequest reportRequest, List<IResource> resources)
        {
            reportRequest.Type = ReportType.ResourceUsageReport;
            var sql = GetResourceUsageQuery(reportRequest);

            IList<ResourceReportDataItem> resourceReportDataItems;

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                query.SetParameter("dateStart", reportRequest.DateRangeStart);
                query.SetParameter("dateEnd", reportRequest.DateRangeEnd);

                if (reportRequest.InstitutionId > 0)
                {
                    query.SetParameter("institutionId", reportRequest.InstitutionId);
                }

                if (reportRequest.ResourceId > 0)
                {
                    query.SetParameter("resourceId", reportRequest.ResourceId);
                }

                if (reportRequest.PublisherId > 0)
                {
                    query.SetParameter("publisherId", reportRequest.PublisherId);
                }

                if (reportRequest.PracticeAreaId > 0)
                {
                    query.SetParameter("practiceAreaId", reportRequest.PracticeAreaId);
                }

                if (reportRequest.SpecialtyId > 0)
                {
                    query.SetParameter("specialtyId", reportRequest.SpecialtyId);
                }

                if (reportRequest.InstitutionId == 0)
                {
                    query.SetTimeout(360);
                }

                resourceReportDataItems =
                    query.SetResultTransformer(Transformers.AliasToBean(typeof(ResourceReportDataItem)))
                        .List<ResourceReportDataItem>();
            }

            LogReportRequest(reportRequest);
            return ProcessResourceReportDataItems(resourceReportDataItems, resources, reportRequest);
        }

        public List<AnnualFeeReportDataItem> GetAnnualFeeReportDataItems(ReportRequest reportRequest)
        {
            reportRequest.Type = ReportType.AnnualFeeReport;

            IList<AnnualFeeReportDataItem> annualFeeReportDataItems;
            var sql = GetAnnualFeeReportSql(reportRequest);

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);
                query.SetParameter("dateStart", reportRequest.DateRangeStart);
                query.SetParameter("dateEnd", reportRequest.DateRangeEnd);

                annualFeeReportDataItems = query
                    .SetResultTransformer(Transformers.AliasToBean(typeof(AnnualFeeReportDataItem)))
                    .List<AnnualFeeReportDataItem>();
            }

            LogReportRequest(reportRequest);
            return annualFeeReportDataItems.ToList();
        }


        public void SaveSavedReport(DateTime lastUpdated, int savedReportId)
        {
            var sql = new StringBuilder()
                .Append("update tSavedReports ")
                .Append("set dtLastUpdate = :lastUpdated ")
                .Append("where iReportId = :reportId ")
                .ToString();
            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);
                query.SetParameter("lastUpdated", lastUpdated);
                query.SetParameter("reportId", savedReportId);

                query.List();
            }
        }

        public List<SavedReport> GetSavedReports(ReportFrequency frequency)
        {
            var query = from s in _savedReports
                join i in _institutions on s.InstitutionId equals i.Id
                join u in _users on s.UserId equals u.Id
                where
                    (i.AccountStatusId == (int)AccountStatus.Active ||
                     (i.AccountStatusId == (int)AccountStatus.Trial && i.Trial.EndDate > DateTime.Now))
                    && s.Frequency == (int)frequency && i.RecordStatus
                    && u.RecordStatus && (u.ExpirationDate == null || u.ExpirationDate > DateTime.Now)
                select s;


            return query.ToList();
        }

        public List<TurnawayResource> GetTurnawayResources2(string reportDatabaseName, string r2DatabaseName)
        {
            var sql = GetYesturdayTurnaways(reportDatabaseName, r2DatabaseName);

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                var results = query.SetResultTransformer(Transformers.AliasToBean(typeof(TurnawayDate)))
                    .List<TurnawayDate>();

                var turnaways = new List<TurnawayResource>();
                foreach (var turnawayDate in results)
                {
                    var turnaway = turnaways.FirstOrDefault(x =>
                        x.ResourceId == turnawayDate.ResourceId && x.InstitutionId == turnawayDate.InstitutionId);
                    if (turnaway == null)
                    {
                        turnaway = new TurnawayResource
                        {
                            InstitutionId = turnawayDate.InstitutionId,
                            ResourceId = turnawayDate.ResourceId,
                            TurnawayDates = new List<TurnawayDate>()
                        };
                        turnaway.TurnawayDates.Add(turnawayDate);
                        turnaways.Add(turnaway);
                    }
                    else
                    {
                        turnaway.TurnawayDates.Add(turnawayDate);
                    }
                }

                //_log.Info(string.Join("  \r\n  ", turnaways.Select(y=> $"InstitutionId:{y.InstitutionId}_ResourceId:{y.ResourceId}_Count:{y.TurnawayDates.Count}" )));
                return turnaways;
            }
        }

        public PublisherReportCounts GetPublisherReportCounts(ReportRequest reportRequest, List<IResource> resources)
        {
            reportRequest.Type = ReportType.PublisherUser;
            var counts = new PublisherReportCounts();

            var sql = new StringBuilder()
                //.Append(" select (select count(iResourceId) ")
                //.Append(" from tResource r join tPublisher p on p.iPublisherId = r.iPublisherId")
                //.Append(" where (p.iPublisherId = :publisherId or p.iConsolidatedPublisherId = :publisherId) and iResourceStatusId in (6,7) ")
                //.Append(" and dtRISReleaseDate  between :dateStart and :dateEnd and r.tiRecordStatus = 1) as 'NumberOfNewTitles', ")
                //.Append(" cc.NumberOfLicenses, cc.TotalSales from ")
                //.Append(" (select sum(ci.iNumberOfLicenses) 'NumberOfLicenses', sum(ci.iNumberOfLicenses*ci.decDiscountPrice) 'TotalSales' ")
                //.Append(" from tResource r ")
                //.Append(" join tPublisher p on p.iPublisherId = r.iPublisherId ")
                //.Append(" join tcartitem ci on r.iResourceId = ci.iResourceId ")
                //.Append(" join tCart c on ci.iCartId = c.iCartId ")
                //.Append(" where (r.iPublisherId = :publisherId  or p.iConsolidatedPublisherId = :publisherId) ")
                //.Append(" and ci.dtPurchaseDate between :dateStart and :dateEnd and c.tiRecordStatus = 1 and ci.tiRecordStatus = 1) as cc ");
                .Append(
                    " 	select distinct t.iResourceId as ResourceId, max(NumberOfLicenses) as Licenses, max(t.TotalSales) as TotalSales, CAST(max(t.isNewTitle) AS BIT) as IsNewTitle  ")
                .Append(" 	from ( ")
                .Append(" 		select r.iResourceId, ISNULL(sum(ci.iNumberOfLicenses), 0) 'NumberOfLicenses' ")
                //.Append(" 			, ISNULL(sum(ci.iNumberOfLicenses*ci.decDiscountPrice), 0) 'TotalSales', 0 as isNewTitle ")
                .Append(
                    " 	 	    , ISNULL(SUM(CASE WHEN ci.tiBundle = 1 then ci.decDiscountPrice else ci.iNumberOfLicenses * ci.decDiscountPrice end), 0) AS 'TotalSales' ")
                .Append(" 			, 0 as isNewTitle ")
                .Append(" 			from tResource r  ")
                .Append(" 			join tPublisher p on p.iPublisherId = r.iPublisherId and p.tiRecordStatus = 1  ")
                .Append(" 			join tOrderHistoryItem ci on r.iResourceId = ci.iResourceId  and ci.tiRecordStatus = 1 ")
                .Append(" 			join tOrderHistory c on ci.iOrderHistoryId = c.iOrderHistoryId and c.tiRecordStatus = 1 ")
                .Append(
                    " 			join tInstitution i on c.iInstitutionId = i.iInstitutionId and i.tiHouseAcct = 0 and i.tiRecordStatus = 1 and i.iInstitutionAcctStatusId = 1 ")
                .Append(" 			where (r.iPublisherId = :publisherId  or p.iConsolidatedPublisherId = :publisherId)  ")
                .Append(" 				and ci.dtCreationDate between :dateStart and :dateEnd ")
                .Append(" 			group by r.iResourceId ")
                .Append(" 		union  ")
                .Append(" 		select r.iResourceId, 0 as 'NumberOfLicenses', 0 as 'TotalSales', 1 as isNewTitle ")
                .Append(" 			from tResource r join tPublisher p on p.iPublisherId = r.iPublisherId ")
                .Append(
                    " 			where (p.iPublisherId = :publisherId or p.iConsolidatedPublisherId = :publisherId) and iResourceStatusId in (6,7)  ")
                .Append(" 			and dtRISReleaseDate  between :dateStart and :dateEnd and r.tiRecordStatus = 1 ")
                .Append(" 			group by r.iResourceId ")
                .Append(" 	) as t ")
                .Append(" 	group by t.iResourceId ")
                .Append(" 	order by 2 desc ");

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql.ToString());

                query.SetParameter("dateStart", reportRequest.DateRangeStart);
                query.SetParameter("dateEnd", reportRequest.DateRangeEnd);
                query.SetParameter("publisherId", reportRequest.PublisherId);

                var publisherReportCounts =
                    query.SetResultTransformer(Transformers.AliasToBean(typeof(PublisherReportCount)))
                        .List<PublisherReportCount>();

                counts.Items = new List<PublisherReportCount>();
                foreach (var publisherReportCount in publisherReportCounts)
                {
                    var resource = resources.FirstOrDefault(x => x.Id == publisherReportCount.ResourceId);
                    if (resource != null)
                    {
                        counts.NewTitlesCount += publisherReportCount.IsNewTitle ? 1 : 0;
                        counts.TitlesSoldCount += publisherReportCount.Licenses;
                        counts.TitleSales += publisherReportCount.TotalSales;
                        publisherReportCount.Resource = resource;
                        counts.Items.Add(publisherReportCount);
                    }
                }

                counts.Items = counts.Items.OrderByDescending(x => x.Licenses).ToList();
            }

            LogReportRequest(reportRequest);
            return counts;
        }


        public SalesReportItems GetSalesReport(ReportRequest reportRequest, List<IResource> resources)
        {
            reportRequest.Type = ReportType.SalesReport;
            var salesReport = new SalesReportItems();

            var sql = new StringBuilder()
                .Append(
                    " 	select r.iResourceId 'ResourceId', ISNULL(sum(case when r.tiFreeResource = 1 then 1 else  ci.iNumberOfLicenses end), 0) 'Licenses' ")
                .Append(
                    " 	 	, ISNULL(SUM(CASE WHEN ci.tiBundle = 1 then ci.decDiscountPrice else ci.iNumberOfLicenses * ci.decDiscountPrice end), 0) AS 'TotalSales' ")
                .Append(
                    " 	 	, r.dtRISReleaseDate 'ReleaseDate', r.dtResourcePublicationDate 'CopyRightDate', r.vchResourceSortTitle 'SortTitle' ")
                .Append(" 	 	from tResource r ");
            if (reportRequest.PracticeAreaId > 0)
            {
                sql.Append(
                    " 	 	left join tResourcePracticeArea rpa on r.iResourceId = rpa.iResourceId and rpa.tiRecordStatus = 1 and rpa.iPracticeAreaId = :practiceAreaId");
            }

            if (reportRequest.SpecialtyId > 0)
            {
                sql.Append(
                    " 	 	left join tResourceSpecialty rs on r.iResourceId = rs.iResourceId and rs.tiRecordStatus = 1 and rs.iSpecialtyId = :specialtyId");
            }

            sql.Append(" 	 	join tPublisher p on p.iPublisherId = r.iPublisherId and p.tiRecordStatus = 1 ")
                .Append(" 	 	join tOrderHistoryItem ci on r.iResourceId = ci.iResourceId and ci.tiRecordStatus = 1 ")
                .Append(" 	 	join tOrderHistory c on ci.iOrderHistoryId = c.iOrderHistoryId and c.tiRecordStatus = 1 ")
                .Append(
                    " 	 	join tInstitution i on c.iInstitutionId = i.iInstitutionId and i.tiHouseAcct = 0 and i.tiRecordStatus = 1 and i.iInstitutionAcctStatusId = 1 ")
                .Append(" 	 	join tTerritory t on i.iTerritoryId = t.iTerritoryId and t.tiRecordStatus = 1")
                .Append(" 		where c.dtPurchaseDate between :dateStart and :dateEnd ")
                .Append(reportRequest.Status > 0
                    ? $" 	 	and r.iResourceStatusId = {reportRequest.Status} "
                    : " 	 	and r.iResourceStatusId in (6,7,8) ");
            if (reportRequest.PublisherId > 0)
            {
                sql.Append(" and (r.iPublisherId = :publisherId  or p.iConsolidatedPublisherId = :publisherId) ");
            }

            if (!string.IsNullOrWhiteSpace(reportRequest.TerritoryCode))
            {
                sql.Append(" and t.vchTerritoryCode = :territoryCode ");
            }

            if (reportRequest.InstitutionTypeId > 0)
            {
                sql.Append(" and i.iInstitutionTypeId = :institutionTypeId ");
            }

            if (reportRequest.ResourceId > 0)
            {
                sql.Append(" and r.iResourceId = :resourceId ");
            }

            sql.Append(
                " group by r.iResourceId, r.vchResourceSortTitle, r.dtRISReleaseDate, r.dtResourcePublicationDate ");


            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql.ToString());
                query.SetTimeout(360);
                query.SetParameter("dateStart", reportRequest.DateRangeStart);
                query.SetParameter("dateEnd", reportRequest.DateRangeEnd);
                if (reportRequest.PublisherId > 0)
                {
                    query.SetParameter("publisherId", reportRequest.PublisherId);
                }

                if (!string.IsNullOrWhiteSpace(reportRequest.TerritoryCode))
                {
                    query.SetParameter("territoryCode", reportRequest.TerritoryCode);
                }

                if (reportRequest.InstitutionTypeId > 0)
                {
                    query.SetParameter("institutionTypeId", reportRequest.InstitutionTypeId);
                }

                if (reportRequest.ResourceId > 0)
                {
                    query.SetParameter("resourceId", reportRequest.ResourceId);
                }

                if (reportRequest.PracticeAreaId > 0)
                {
                    query.SetParameter("practiceAreaId", reportRequest.PracticeAreaId);
                }

                if (reportRequest.SpecialtyId > 0)
                {
                    query.SetParameter("specialtyId", reportRequest.SpecialtyId);
                }

                var salesReportItems = query.SetResultTransformer(Transformers.AliasToBean(typeof(SalesReportItem)))
                    .List<SalesReportItem>();

                salesReport.Items = new List<SalesReportItem>();
                foreach (var salesReportItem in salesReportItems)
                {
                    var resource = resources.FirstOrDefault(x => x.Id == salesReportItem.ResourceId);
                    if (resource != null)
                    {
                        salesReport.TitlesSoldCount += salesReportItem.Licenses;
                        salesReport.TitleSales += salesReportItem.TotalSales;
                        salesReportItem.Resource = resource;
                        salesReport.Items.Add(salesReportItem);
                    }
                }

                switch (reportRequest.SortBy)
                {
                    case ReportSortBy.Copyright:
                        salesReport.Items = salesReport.Items.OrderByDescending(x => x.CopyRightDate).ToList();
                        break;
                    case ReportSortBy.ReleaseDate:
                        salesReport.Items = salesReport.Items.OrderByDescending(x => x.ReleaseDate).ToList();
                        break;
                    case ReportSortBy.Title:
                        //SortTitle
                        salesReport.Items = salesReport.Items.OrderBy(x => x.SortTitle).ToList();
                        break;
                    case ReportSortBy.LicensesSold:
                    default:
                        salesReport.Items = salesReport.Items.OrderByDescending(x => x.Licenses).ToList();
                        break;
                }
            }

            LogReportRequest(reportRequest);
            return salesReport;
        }

        public List<PdaCountsReportDataItem> GetPdaReportCounts(ReportRequest reportRequest)
        {
            reportRequest.Type = ReportType.PdaCountsReport;

            #region SQL

            const string sql = @"
select i.iInstitutionId as InstitutionId, vchInstitutionAcctNum as AccountNumber, ltrim(vchInstitutionName) as InstitutionName, t.vchTerritoryCode as TerritoryCode
     , sum(PdaPurchasedResources) as PdaPurchasedResources, sum(PdaPurchasedLicenses) as PdaPurchasedLicenses
     , sum(FirmPurchasedResources) as FirmPurchasedResources, sum(FirmPurchasedLicenses) as FirmPurchasedLicenses
     , sum(PdaTitlesAdded) as PdaTitlesAdded, sum(PdaTitlesDeleted) as PdaTitlesDeleted, sum(PdaTitleViews) as PdaTitleViews
     , sum(PdaTitlesAddedToCart) as PdaTitlesAddedToCart, sum(PdaTitlesAddedByRule) as PdaTitlesAddedByRule
     , sum(PdaWizardTitlesAddedToCart) as PdaWizardTitlesAddedToCart, sum(PdaWizardTitlesDeleted) as PdaWizardTitlesDeleted
     , sum(PdaWizardPurchasedResources) as PdaWizardPurchasedResources, sum(PdaWizardPurchasedLicenses) as PdaWizardPurchasedLicenses
     , sum(PdaWizardTitleViews) as PdaWizardTitleViews
     , sum(PdaRuleCount) as [PdaRuleCount], sum(PdaRulesFuture) as [PdaRulesFuture], sum(PdaRulesNewEditionFirm) as [PdaRulesNewEditionFirm], sum(PdaRulesNewEditionPda) as [PdaRulesNewEditionPda]
from (
        -- Gets PDA resources puchased & PDA license purchased
        select c.iInstitutionId, count(ci.iNumberOfLicenses) as PdaPurchasedResources, sum(ci.iNumberOfLicenses) as PdaPurchasedLicenses
             , 0 as FirmPurchasedResources, 0 as FirmPurchasedLicenses, 0 as PdaTitlesAdded, 0 as PdaTitlesDeleted, 0 as PdaTitleViews
             , 0 as PdaTitlesAddedToCart, 0 as PdaTitlesAddedByRule, 0 as PdaWizardTitlesAddedToCart, 0 as PdaWizardTitlesDeleted
             , 0 as PdaWizardPurchasedResources, 0 as PdaWizardPurchasedLicenses, 0 as PdaWizardTitleViews
             , 0 as [PdaRuleCount], 0 as [PdaRulesFuture], 0 as [PdaRulesNewEditionFirm], 0 as [PdaRulesNewEditionPda]
        from   tCart c
         join  tCartItem ci on c.iCartId = ci.iCartId and ci.tiLicenseOriginalSourceId = 2 and ci.tiRecordStatus = 1 and ci.iNumberOfLicenses > 0
        where  c.dtPurchaseDate >= :StartDate and  c.dtPurchaseDate <= :EndDate
          and  c.tiProcessed = 1 and c.tiRecordStatus = 1
        group by c.iInstitutionId

        union

        -- Get Firm resources purchased & Firm licenses purchased
        select c.iInstitutionId, 0 as PdaPurchasedResources, 0 as PdaPurchasedLicenses
             , count(ci.iNumberOfLicenses) as FirmPurchasedResources, sum(case when (r.tiFreeResource = 1) then 1 else ci.iNumberOfLicenses end) as FirmPurchasedLicenses
             , 0 as PdaTitlesAdded, 0 as PdaTitlesDeleted, 0 as PdaTitleViews
             , 0 as PdaTitlesAddedToCart, 0 as PdaTitlesAddedByRule, 0 as PdaWizardTitlesAddedToCart, 0 as PdaWizardTitlesDeleted
             , 0 as PdaWizardPurchasedResources, 0 as PdaWizardPurchasedLicenses, 0 as PdaWizardTitleViews
             , 0 as [PdaRuleCount], 0 as [PdaRulesFuture], 0 as [PdaRulesNewEditionFirm], 0 as [PdaRulesNewEditionPda]
        from   tCart c
         join  tCartItem ci on c.iCartId = ci.iCartId and ci.tiLicenseOriginalSourceId = 1 and ci.tiRecordStatus = 1 and ci.iNumberOfLicenses > 0
         join  tResource r on r.iResourceId = ci.iResourceId
          and  exists (select 1 from tInstitutionResourceLicense irl where irl.iInstitutionId = c.iInstitutionId and irl.tiLicenseOriginalSourceId = 2)
        where  c.dtPurchaseDate >= :StartDate and  c.dtPurchaseDate <= :EndDate
          and  c.tiProcessed = 1 and c.tiRecordStatus = 1
        group by c.iInstitutionId

        union

        -- Get PDA titles added
        select irl.iInstitutionId, 0 as PdaPurchasedResources, 0 as PdaPurchasedLicenses, 0 as FirmPurchasedResources, 0 as FirmPurchasedLicenses
             , count(irl.iResourceId) as PdaTitlesAdded, 0 as PdaTitlesDeleted, 0 as PdaTitleViews
             , 0 as PdaTitlesAddedToCart, 0 as PdaTitlesAddedByRule, 0 as PdaWizardTitlesAddedToCart, 0 as PdaWizardTitlesDeleted
             , 0 as PdaWizardPurchasedResources, 0 as PdaWizardPurchasedLicenses, 0 as PdaWizardTitleViews
             , 0 as [PdaRuleCount], 0 as [PdaRulesFuture], 0 as [PdaRulesNewEditionFirm], 0 as [PdaRulesNewEditionPda]
        from   tInstitutionResourceLicense irl
        where  irl.dtPdaAddedDate >= :StartDate and irl.dtPdaAddedDate <= :EndDate
          and  irl.tiRecordStatus = 1
        group by irl.iInstitutionId

        union

        -- Get PDA titles deleted
        select irl.iInstitutionId, 0 as PdaPurchasedResources, 0 as PdaPurchasedLicenses, 0 as FirmPurchasedResources, 0 as FirmPurchasedLicenses
             , 0 as PdaTitlesAdded, count(irl.iResourceId) as PdaTitlesDeleted, 0 as PdaTitleViews
             , 0 as PdaTitlesAddedToCart, 0 as PdaTitlesAddedByRule, 0 as PdaWizardTitlesAddedToCart, 0 as PdaWizardTitlesDeleted
             , 0 as PdaWizardPurchasedResources, 0 as PdaWizardPurchasedLicenses, 0 as PdaWizardTitleViews
             , 0 as [PdaRuleCount], 0 as [PdaRulesFuture], 0 as [PdaRulesNewEditionFirm], 0 as [PdaRulesNewEditionPda]
        from   tInstitutionResourceLicense irl
        where  irl.dtPdaDeletedDate >= :StartDate and irl.dtPdaDeletedDate <= :EndDate
          and  irl.tiRecordStatus = 1
        group by irl.iInstitutionId

        union

        -- Gets PDA Views
        select ira.iInstitutionId, 0 as PdaPurchasedResources, 0 as PdaPurchasedLicenses, 0 as FirmPurchasedResources, 0 as FirmPurchasedLicenses
             , 0 as PdaTitlesAdded, 0 as PdaTitlesDeleted, count(ira.iResourceId) as PdaTitleViews
             , 0 as PdaTitlesAddedToCart, 0 as PdaTitlesAddedByRule, 0 as PdaWizardTitlesAddedToCart, 0 as PdaWizardTitlesDeleted
             , 0 as PdaWizardPurchasedResources, 0 as PdaWizardPurchasedLicenses, 0 as PdaWizardTitleViews
             , 0 as [PdaRuleCount], 0 as [PdaRulesFuture], 0 as [PdaRulesNewEditionFirm], 0 as [PdaRulesNewEditionPda]
        from   tInstitutionResourceAudit ira
        where  ira.dtCreationDate >= :StartDate and ira.dtCreationDate <= :EndDate
          and  ira.iInstitutionResourceAuditTypeId = 9
        group by ira.iInstitutionId

        union

        -- Get PDA titles Added to Cart
        select irl.iInstitutionId, 0 as PdaPurchasedResources, 0 as PdaPurchasedLicenses, 0 as FirmPurchasedResources, 0 as FirmPurchasedLicenses
             , 0 as PdaTitlesAdded, 0 as PdaTitlesDeleted, 0 as PdaTitleViews
             , count(irl.iResourceId) as PdaTitlesAddedToCart, 0 as PdaTitlesAddedByRule, 0 as PdaWizardTitlesAddedToCart, 0 as PdaWizardTitlesDeleted
             , 0 as PdaWizardPurchasedResources, 0 as PdaWizardPurchasedLicenses, 0 as PdaWizardTitleViews
             , 0 as [PdaRuleCount], 0 as [PdaRulesFuture], 0 as [PdaRulesNewEditionFirm], 0 as [PdaRulesNewEditionPda]
        from   tInstitutionResourceLicense irl
        where  irl.dtPdaAddedToCartDate >= :StartDate and irl.dtPdaAddedToCartDate <= :EndDate
          and  irl.tiRecordStatus = 1
        group by irl.iInstitutionId

        union

        -- Get PDA Wizard titles Added to Cart
        select irl.iInstitutionId, 0 as PdaPurchasedResources, 0 as PdaPurchasedLicenses, 0 as FirmPurchasedResources, 0 as FirmPurchasedLicenses
             , 0 as PdaTitlesAdded, 0 as PdaTitlesDeleted, 0 as PdaTitleViews
             , 0 as PdaTitlesAddedToCart, count(irl.iResourceId) as PdaTitlesAddedByRule, 0 as PdaWizardTitlesAddedToCart, 0 as PdaWizardTitlesDeleted
             , 0 as PdaWizardPurchasedResources, 0 as PdaWizardPurchasedLicenses, 0 as PdaWizardTitleViews
             , 0 as [PdaRuleCount], 0 as [PdaRulesFuture], 0 as [PdaRulesNewEditionFirm], 0 as [PdaRulesNewEditionPda]
        from   tInstitutionResourceLicense irl
        where  irl.dtPdaRuleDateAdded >= :StartDate and irl.dtPdaRuleDateAdded <= :EndDate
          and  irl.tiRecordStatus = 1
        group by irl.iInstitutionId

        union

        -- Get PDA Wizard titles Added to Cart
        select irl.iInstitutionId, 0 as PdaPurchasedResources, 0 as PdaPurchasedLicenses, 0 as FirmPurchasedResources, 0 as FirmPurchasedLicenses
             , 0 as PdaTitlesAdded, 0 as PdaTitlesDeleted, 0 as PdaTitleViews
             , 0 as PdaTitlesAddedToCart, 0 as PdaTitlesAddedByRule, count(irl.iResourceId) as PdaWizardTitlesAddedToCart, 0 as PdaWizardTitlesDeleted
             , 0 as PdaWizardPurchasedResources, 0 as PdaWizardPurchasedLicenses, 0 as PdaWizardTitleViews
             , 0 as [PdaRuleCount], 0 as [PdaRulesFuture], 0 as [PdaRulesNewEditionFirm], 0 as [PdaRulesNewEditionPda]
        from   tInstitutionResourceLicense irl
        where  irl.dtPdaAddedToCartDate >= :StartDate and irl.dtPdaAddedToCartDate <= :EndDate
          and  irl.tiRecordStatus = 1 and irl.dtPdaRuleDateAdded is not null
        group by irl.iInstitutionId

        union

        -- Get PDA Wizard titles Deleted to Cart
        select irl.iInstitutionId, 0 as PdaPurchasedResources, 0 as PdaPurchasedLicenses, 0 as FirmPurchasedResources, 0 as FirmPurchasedLicenses
             , 0 as PdaTitlesAdded, 0 as PdaTitlesDeleted, 0 as PdaTitleViews
             , 0 as PdaTitlesAddedToCart, 0 as PdaTitlesAddedByRule, 0 as PdaWizardTitlesAddedToCart, count(irl.iResourceId) as PdaWizardTitlesDeleted
             , 0 as PdaWizardPurchasedResources, 0 as PdaWizardPurchasedLicenses, 0 as PdaWizardTitleViews
             , 0 as [PdaRuleCount], 0 as [PdaRulesFuture], 0 as [PdaRulesNewEditionFirm], 0 as [PdaRulesNewEditionPda]
        from   tInstitutionResourceLicense irl
        where  irl.dtPdaDeletedDate >= :StartDate and irl.dtPdaDeletedDate <= :EndDate
          and  irl.tiRecordStatus = 1 and irl.dtPdaRuleDateAdded is not null
        group by irl.iInstitutionId

        union

        -- Gets PDA Wizard resources puchased & PDA Wizard license purchased
        select c.iInstitutionId, 0 as PdaPurchasedResources, 0 as PdaPurchasedLicenses
             , 0 as FirmPurchasedResources, 0 as FirmPurchasedLicenses, 0 as PdaTitlesAdded, 0 as PdaTitlesDeleted, 0 as PdaTitleViews
             , 0 as PdaTitlesAddedToCart, 0 as PdaTitlesAddedByRule, 0 as PdaWizardTitlesAddedToCart, 0 as PdaWizardTitlesDeleted
             , count(ci.iNumberOfLicenses) as PdaWizardPurchasedResources, sum(ci.iNumberOfLicenses) as PdaWizardPurchasedLicenses, 0 as PdaWizardTitleViews
             , 0 as [PdaRuleCount], 0 as [PdaRulesFuture], 0 as [PdaRulesNewEditionFirm], 0 as [PdaRulesNewEditionPda]
        from   tCart c
         join  tCartItem ci on c.iCartId = ci.iCartId and ci.tiLicenseOriginalSourceId = 2 and ci.tiRecordStatus = 1 and ci.iNumberOfLicenses > 0
         join  tInstitutionResourceLicense irl on irl.iInstitutionId = c.iInstitutionId and irl.iResourceId = ci.iResourceId and irl.dtPdaRuleDateAdded is not null
        where  c.dtPurchaseDate >= :StartDate and  c.dtPurchaseDate <= :EndDate
          and  c.tiProcessed = 1 and c.tiRecordStatus = 1
        group by c.iInstitutionId

        union

        -- Gets PDA Wizard Views
        select ira.iInstitutionId, 0 as PdaPurchasedResources, 0 as PdaPurchasedLicenses, 0 as FirmPurchasedResources, 0 as FirmPurchasedLicenses
             , 0 as PdaTitlesAdded, 0 as PdaTitlesDeleted, 0 as PdaTitleViews
             , 0 as PdaTitlesAddedToCart, 0 as PdaTitlesAddedByRule, 0 as PdaWizardTitlesAddedToCart, 0 as PdaWizardTitlesDeleted
             , 0 as PdaWizardPurchasedResources, 0 as PdaWizardPurchasedLicenses, count(ira.iResourceId) as PdaWizardTitleViews
             , 0 as [PdaRuleCount], 0 as [PdaRulesFuture], 0 as [PdaRulesNewEditionFirm], 0 as [PdaRulesNewEditionPda]
        from   tInstitutionResourceAudit ira
         join  tInstitutionResourceLicense irl on ira.iInstitutionId = irl.iInstitutionId and ira.iResourceId = irl.iResourceId and irl.dtPdaRuleDateAdded is not null
        where  ira.dtCreationDate >= :StartDate and ira.dtCreationDate <= :EndDate
          and  ira.iInstitutionResourceAuditTypeId = 9
        group by ira.iInstitutionId

        union

        -- Get PDA Rules
        select pr.iInstitutionId, 0 as PdaPurchasedResources, 0 as PdaPurchasedLicenses, 0 as FirmPurchasedResources, 0 as FirmPurchasedLicenses
             , 0 as PdaTitlesAdded, 0 as PdaTitlesDeleted, 0 as PdaTitleViews
             , 0 as PdaTitlesAddedToCart, 0 as PdaTitlesAddedByRule, 0 as PdaWizardTitlesAddedToCart, 0 as PdaWizardTitlesDeleted
             , 0 as PdaWizardPurchasedResources, 0 as PdaWizardPurchasedLicenses, 0 as PdaWizardTitleViews
             , count(pr.iPdaRuleId) as [PdaRuleCount], count(pr.tiFuture) as [PdaRulesFuture], count(pr.tiNewEditionFirm) as [PdaRulesNewEditionFirm], count(pr.tiNewEditionPda) as [PdaRulesNewEditionPda]
        from   tPdaRule pr
        where  pr.tiRecordStatus = 1 and pr.dtCreationDate < :EndDate
        group by pr.iInstitutionId

    ) as counts
 join  tInstitution i on i.iInstitutionId = counts.iInstitutionId and i.tiRecordStatus = 1 and i.tiHouseAcct = 0
 left join tInstitutionTerritory ir on counts.iInstitutionId = ir.iInstitutionId and ir.tiRecordStatus = 1
 left join tTerritory t on i.iTerritoryId = t.iTerritoryId
";

            #endregion

            IList<PdaCountsReportDataItem> results;
            using (var uow = _unitOfWorkProvider.Start())
            {
                var sqlBuilder = new StringBuilder()
                    .AppendLine(sql);
                if (!string.IsNullOrWhiteSpace(reportRequest.TerritoryCode))
                {
                    sqlBuilder.AppendLine("where vchTerritoryCode = :TerritoryCode ");
                }

                sqlBuilder.AppendLine(
                    "group by i.iInstitutionId, i.vchInstitutionAcctNum, i.vchInstitutionName, t.vchTerritoryCode");
                sqlBuilder.AppendLine("order by ltrim(i.vchInstitutionName)");

                var query = uow.Session.CreateSQLQuery(sqlBuilder.ToString());

                query.SetParameter("StartDate", reportRequest.DateRangeStart);
                query.SetParameter("EndDate", reportRequest.DateRangeEnd);

                if (!string.IsNullOrWhiteSpace(reportRequest.TerritoryCode))
                {
                    query.SetParameter("TerritoryCode", reportRequest.TerritoryCode);
                }

                results = query.SetResultTransformer(Transformers.AliasToBean(typeof(PdaCountsReportDataItem)))
                    .List<PdaCountsReportDataItem>();
            }

            LogReportRequest(reportRequest);
            return results.ToList();
        }


        public bool LogReportRequest(ReportRequest reportRequest)
        {
            if (!IsValidDate(reportRequest.DateRangeStart) || !IsValidDate(reportRequest.DateRangeEnd))
            {
                return false;
            }

            var log = new ReportLog
            {
                Type = reportRequest.Type,
                Period = reportRequest.Period,
                DateRangeStart = reportRequest.DateRangeStart,
                DateRangeEnd = reportRequest.DateRangeEnd,
                IncludePdaTitles = reportRequest.IncludePdaTitles,
                IncludePurchasedTitles = reportRequest.IncludePurchasedTitles,
                IncludeTocTitles = reportRequest.IncludeTocTitles,
                IncludeTrialStats = reportRequest.IncludeTrialStats,
                TerritoryCode = reportRequest.TerritoryCode,
                SortBy = reportRequest.SortBy,
                IsDefaultQuery = reportRequest.IsDefaultRequest()
            };

            if (reportRequest.PracticeAreaId > 0)
            {
                log.PracticeAreaId = reportRequest.PracticeAreaId;
            }

            if (reportRequest.SpecialtyId > 0)
            {
                log.SpecialtyId = reportRequest.SpecialtyId;
            }

            if (reportRequest.PublisherId > 0)
            {
                log.PublisherId = reportRequest.PublisherId;
            }

            if (reportRequest.ResourceId > 0)
            {
                log.ResourceId = reportRequest.ResourceId;
            }

            if (reportRequest.InstitutionId > 0)
            {
                log.InstitutionId = reportRequest.InstitutionId;
            }

            if (reportRequest.InstitutionTypeId > 0)
            {
                log.InstitutionTypeId = reportRequest.InstitutionTypeId;
            }

            var ipFilters = reportRequest.GetCondensedRanges().ToList();
            if (ipFilters.Any())
            {
                log.IpFilter = string.Join(" || ", ipFilters.Select(x => x.ToAuditString()));
            }


            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.Save(log);
                    uow.Commit();
                    transaction.Commit();
                }
            }

            return true;
        }

        public List<ResourceRequestItem> GetResourceRequestItems(ResourceAccessReportRequest reportRequest)
        {
            var sqlBase = @"
select urr.iInstitutionId InstitutionId, urr.iResourceId ResourceId, count(urr.iResourceId) RequestCount, max(urr.dtCreationDate) LastRequestDate
    , oh.decDiscountPrice PurchasePrice, oh.iOrderHistoryId OrderHistoryId, oh.dtPurchaseDate PurchaseDate
    , cc.iCartId CartId, ac.iAutomatedCartId AutomatedCartId, cc.dtCreationDate AddedDate
    , i.iTerritoryId TerritoryId, i.iInstitutionTypeId InstitutionTypeId, ac.vchCartName AutomatedCartName
        from tUserResourceRequest urr
        join tInstitution i on urr.iInstitutionId = i.iInstitutionId
        join tResource r on urr.iResourceId = r.iResourceId and r.iResourceStatusId in (6,8)
        left join (
            select o.iInstitutionId, oi.iResourceId, o.dtPurchaseDate, oi.decDiscountPrice, o.iOrderHistoryId
            from tOrderHistory o
            join tOrderHistoryItem oi on o.iOrderHistoryId = oi.iOrderHistoryId and oi.tiRecordStatus = 1
            join (
                select min(oh.dtPurchaseDate) dtPurchaseDate, oh.iInstitutionId, ohi.iResourceId
                from tOrderHistory oh
                join tOrderHistoryItem ohi on oh.iOrderHistoryId = ohi.iOrderHistoryId and ohi.tiRecordStatus = 1
                group by oh.iInstitutionId, ohi.iResourceId
            ) zz on zz.dtPurchaseDate = o.dtPurchaseDate and zz.iResourceId = oi.iResourceId and o.iInstitutionId = zz.iInstitutionId
            where o.tiRecordStatus = 1
            group by o.iInstitutionId, oi.iResourceId, o.dtPurchaseDate, oi.decDiscountPrice, o.iOrderHistoryId
        ) oh on urr.iInstitutionId = oh.iInstitutionId and oh.iResourceId = r.iResourceId
        left join (
            select c.iInstitutionId, c.iCartId, ci.iResourceId, ci.dtCreationDate, ci.iCartItemId
            from tCart c
            join tCartItem ci on c.iCartId = ci.iCartId and ci.tiRecordStatus = 1
            where c.tiRecordStatus = 1
            group by c.iInstitutionId, c.iCartId, ci.iResourceId, ci.dtCreationDate, ci.iCartItemId
        ) cc on urr.iInstitutionId = cc.iInstitutionId and cc.iResourceId = r.iResourceId and cc.dtCreationDate > urr.dtCreationDate
        left join (
            select ac.iAutomatedCartId, ac.vchCartName, acr.iCartItemId
            from tAutomatedCartResource acr
            join tAutomatedCartInstitution aci on acr.iAutomatedCartInstitutionId = aci.iAutomatedCartInstitutionId
            join tAutomatedCart ac on aci.iAutomatedCartId = ac.iAutomatedCartId and ac.tiRequested = 1
            group by acr.iCartItemId, ac.iAutomatedCartId, ac.vchCartName
        ) ac on ac.iCartItemId = cc.iCartItemId
    where (oh.dtPurchaseDate is null or (oh.dtPurchaseDate > urr.dtCreationDate))
        and (
                (urr.dtCreationDate >= :StartDate and urr.dtCreationDate <= :EndDate)
                or (oh.dtPurchaseDate >= :StartDate and oh.dtPurchaseDate <= :EndDate)
                or (cc.dtCreationDate >= :StartDate and cc.dtCreationDate <= :EndDate)
            )
";
            var sqlGroupBy = @"
  group by urr.iInstitutionId, i.vchInstitutionName, urr.iResourceId, oh.decDiscountPrice, oh.iOrderHistoryId, cc.iCartId
    , ac.iAutomatedCartId, oh.dtPurchaseDate, cc.dtCreationDate, i.iTerritoryId, i.iInstitutionTypeId, ac.vchCartName
    order by i.vchInstitutionName, max(urr.dtCreationDate) desc, oh.dtPurchaseDate, cc.dtCreationDate
";
            var whereBuilder = new StringBuilder();

            if (reportRequest.TerritoryIds != null)
            {
                whereBuilder.Append($" and i.iTerritoryId in ({string.Join(",", reportRequest.TerritoryIds)}) ");
            }

            if (reportRequest.InstitutionTypeIds != null)
            {
                whereBuilder.Append(
                    $" and i.iInstitutionTypeId in ({string.Join(",", reportRequest.InstitutionTypeIds)}) ");
            }

            if (reportRequest.AccountNumbers != null)
            {
                whereBuilder.Append(
                    $" and i.vchInstitutionAcctNum in ('{string.Join("','", reportRequest.AccountNumbers)}') ");
            }

            var sqlString = $"{sqlBase} {whereBuilder} {sqlGroupBy}";

            List<ResourceRequestItem> items = null;
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    var query = uow.Session.CreateSQLQuery(sqlString);

                    query.SetParameter("StartDate", reportRequest.StartDate);
                    query.SetParameter("EndDate", reportRequest.EndDate);

                    var results = query.SetResultTransformer(Transformers.AliasToBean(typeof(ResourceRequestItem)))
                        .List<ResourceRequestItem>();
                    items = results.ToList();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return items;
        }

        private void SetDailyCounts(ApplicationReportCounts counts, ReportRequest reportRequest)
        {
            var sql = new StringBuilder();
            sql.AppendFormat("select ({0}) as [PageViewCount]",
                GetDailyCountQuery(reportRequest.InstitutionId, "vDailyPageViewCount", "dpvc", "pageView",
                    reportRequest.GetCondensedRanges())).AppendLine();
            sql.AppendFormat(", ({0}) as [SessionCount]",
                GetDailyCountQuery(reportRequest.InstitutionId, "vDailySessionCount", "dpvc", "session",
                    reportRequest.GetCondensedRanges())).AppendLine();
            sql.AppendFormat(", ({0}) as [ContentViewCount]",
                GetDailyCountQuery(reportRequest.InstitutionId, "vDailyContentViewCount", "dcvc",
                    "contentView", reportRequest.GetCondensedRanges())).AppendLine();
            sql.AppendFormat(", ({0} and dcvc.chapterSectionId is null) as [TocOnlyViewCount]",
                GetDailyCountQuery(reportRequest.InstitutionId, "vDailyContentViewCount", "dcvc",
                    "contentView", reportRequest.GetCondensedRanges())).AppendLine();
            sql.AppendFormat(", ({0} and dctc.TurnawayTypeId = 20) as [ConcurrencyCount]",
                GetDailyCountQuery(reportRequest.InstitutionId, "vDailyContentTurnawayCount", "dctc",
                    "contentTurnaway", reportRequest.GetCondensedRanges())).AppendLine();
            sql.AppendFormat(", ({0} and dctc.TurnawayTypeId = 21) as [AccessCount]",
                GetDailyCountQuery(reportRequest.InstitutionId, "vDailyContentTurnawayCount", "dctc",
                    "contentTurnaway", reportRequest.GetCondensedRanges())).AppendLine();
            sql.AppendFormat(", ({0}) as [ActiveSearchCount]",
                GetDailySearchCountQuery(reportRequest.InstitutionId, false, "0", SearchType.Undefined,
                    reportRequest.GetCondensedRanges())).AppendLine();
            sql.AppendFormat(", ({0}) as [ArchivedSearchCount]",
                GetDailySearchCountQuery(reportRequest.InstitutionId, false, "1", SearchType.Undefined,
                    reportRequest.GetCondensedRanges())).AppendLine();
            sql.AppendFormat(", ({0}) as [ImageSearchCount]",
                GetDailySearchCountQuery(reportRequest.InstitutionId, false, null, SearchType.Image,
                    reportRequest.GetCondensedRanges())).AppendLine();
            sql.AppendFormat(", ({0}) as [DrugSearchCount]",
                GetDailySearchCountQuery(reportRequest.InstitutionId, false, null, SearchType.DrugMonograph,
                    reportRequest.GetCondensedRanges())).AppendLine();
            sql.AppendFormat(", ({0}) as [ExtPubMedSearchCount]",
                GetDailySearchCountQuery(reportRequest.InstitutionId, true, null, SearchType.PubMed,
                    reportRequest.GetCondensedRanges())).AppendLine();
            sql.AppendFormat(", ({0}) as [ExtMeshSearchCount]",
                GetDailySearchCountQuery(reportRequest.InstitutionId, true, null, SearchType.Mesh,
                    reportRequest.GetCondensedRanges())).AppendLine();

            //KSH 1/30/2014 PDA Counts Query
            sql.AppendFormat(", {0} ", GetPdaCountQuery(reportRequest.InstitutionId)).AppendLine();

            _log.DebugFormat("sql: {0}", sql);

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql.ToString());

                query.SetParameter("dateStart", reportRequest.DateRangeStart);
                query.SetParameter("dateEnd", reportRequest.DateRangeEnd);

                var x = 0;
                foreach (var ipAddressRange in reportRequest.GetCondensedRanges())
                {
                    query.SetParameter($"ipAddressStart_{x}", ipAddressRange.IpNumberStart);
                    query.SetParameter($"ipAddressEnd_{x}", ipAddressRange.IpNumberEnd);
                    x++;
                }

                if (reportRequest.InstitutionId > 0)
                {
                    query.SetParameter("institutionId", reportRequest.InstitutionId);
                }

                var results = query.List();

                foreach (object[] result in results)
                {
                    // application usage stats
                    counts.PageViewCount = ConvertObjectToInt(result[0]);
                    counts.UserSessionCount = ConvertObjectToInt(result[1]);
                    counts.TotalContentRetrievalCount = ConvertObjectToInt(result[2]);
                    counts.TocOnlyContentRetrievalCount = ConvertObjectToInt(result[3]);
                    counts.ConcurrencyTurnawayCount = ConvertObjectToInt(result[4]);
                    counts.AccessTurnawayCount = ConvertObjectToInt(result[5]);
                    counts.RestrictedContentRetrievalCount = counts.TotalContentRetrievalCount -
                                                             counts.TocOnlyContentRetrievalCount;

                    // search stats
                    counts.SearchActiveCount = ConvertObjectToInt(result[6]);
                    counts.SearchArchiveCount = ConvertObjectToInt(result[7]);
                    counts.SearchImageCount = ConvertObjectToInt(result[8]);
                    counts.SearchDrugCount = ConvertObjectToInt(result[9]);

                    // ext search stats
                    counts.SearchPubMedCount = ConvertObjectToInt(result[10]);
                    counts.SearchMeshCount = ConvertObjectToInt(result[11]);


                    //KSH 1/30/2014 PDA Counts Query
                    counts.PdaTotalCount = ConvertObjectToInt(result[12]);
                    counts.PdaActiveCount = ConvertObjectToInt(result[13]);
                    counts.PdaCartCount = ConvertObjectToInt(result[14]);
                    counts.PdaPurchasedCount = ConvertObjectToInt(result[15]);
                }
            }
        }

        private string GetDailyCountQuery(int institutionId, string viewName, string viewPrefix, string fileNamePrefix,
            IEnumerable<IpAddressRange> ipAddressRanges)
        {
            var sql = new StringBuilder()
                .AppendFormat(" select sum({0}.{1}Count) ", viewPrefix, fileNamePrefix)
                .AppendFormat(" from   {0} {1} ", viewName, viewPrefix);

            sql.AppendFormat(
                institutionId == 0
                    ? " join tInstitution i on i.iInstitutionId = {0}.institutionId and i.tiHouseAcct = 0 "
                    : " left outer join tInstitution i on i.iInstitutionId = {0}.institutionId ", viewPrefix);

            sql.AppendFormat(" where  {0}.{1}Date between :dateStart and :dateEnd ", viewPrefix, fileNamePrefix);


            if (institutionId > 0)
            {
                sql.AppendFormat(" and {0}.institutionId = :institutionId ", viewPrefix);
            }

            sql.Append(GetIpAddressRangeFilterSql(ipAddressRanges, viewPrefix));

            return sql.ToString();
        }

        /// <param name="ipAddressRanges"> </param>
        private string GetDailySearchCountQuery(int institutionId, bool isExternal, string archive,
            SearchType searchType, IEnumerable<IpAddressRange> ipAddressRanges)
        {
            var sql = new StringBuilder()
                .Append("select sum(dsc.searchCount) ")
                .Append("from vDailySearchCount dsc ");
            sql.Append(institutionId == 0
                ? " join tInstitution i on i.iInstitutionId = dsc.institutionId  and i.tiHouseAcct = 0 "
                : " join  tInstitution i on i.iInstitutionId = dsc.institutionId ");
            sql.Append("where dsc.searchDate between :dateStart and :dateEnd ")
                .AppendFormat(" and dsc.isExternal = {0} ", isExternal ? "1" : "0");

            if (!string.IsNullOrWhiteSpace(archive))
            {
                sql.AppendFormat(" and dsc.isArchive = {0} ", archive);
            }

            if (searchType != SearchType.Undefined)
            {
                sql.AppendFormat(" and dsc.searchTypeId = {0} ", (int)searchType);
            }

            sql.Append(institutionId > 0
                ? " and dsc.institutionId = :institutionId "
                : " and (i.tiHouseAcct = 0 or i.tiHouseAcct is null) ");

            sql.Append(GetIpAddressRangeFilterSql(ipAddressRanges, "dsc"));

            return sql.ToString();
        }

        private string GetPdaCountQuery(int institutionId)
        {
            var sql = new StringBuilder()
                .Append("( select count(*) ")
                .Append("    from tInstitutionResourceLicense irl");
            if (institutionId == 0)
            {
                sql.Append(" join tInstitution i on irl.iInstitutionId = i.iInstitutionId and i.tiHouseAcct = 0 ");
            }

            sql.Append("    where irl.tiLicenseOriginalSourceId = 2 ");
            if (institutionId > 0)
            {
                sql.Append(" and irl.iInstitutionId = :institutionId ");
            }

            sql.Append("    and irl.tiRecordStatus = 1 and irl.dtCreationDate between :dateStart and :dateEnd ")
                .Append(") as [Total Pda], ").AppendLine()
                .Append("( select count(*) ")
                .Append("    from tInstitutionResourceLicense irl ");

            if (institutionId == 0)
            {
                sql.Append(" join tInstitution i on irl.iInstitutionId = i.iInstitutionId and i.tiHouseAcct = 0");
            }

            sql.Append("  where irl.tiLicenseOriginalSourceId = 2 and irl.tiRecordStatus = 1 ");

            if (institutionId > 0)
            {
                sql.Append(" and irl.iInstitutionId = :institutionId ");
            }

            sql.Append(
                    "    and  irl.iPdaViewCount <  irl.iPdaMaxViews and  irl.dtPdaDeletedDate is null and  irl.dtResourceNotSaleableDate is null and  irl.dtCreationDate between :dateStart and :dateEnd ")
                .Append(") as [Active Pda], ").AppendLine()
                .Append("( select count(*) from tInstitutionResourceLicense irl ");

            if (institutionId == 0)
            {
                sql.Append(" join tInstitution i on irl.iInstitutionId = i.iInstitutionId and i.tiHouseAcct = 0");
            }

            sql.Append("   where irl.tiLicenseOriginalSourceId = 2 and irl.tiRecordStatus = 1 ");

            if (institutionId > 0)
            {
                sql.Append(" and irl.iInstitutionId = :institutionId ");
            }

            sql.Append(
                    "    and irl.dtPdaAddedToCartDate is not null and irl.dtPdaAddedToCartDate between :dateStart and :dateEnd ")
                .Append(") as [Cart Pda], ").AppendLine()
                .Append("( select count(ci.iCartItemId) ")
                .Append("    from tCart c join tCartItem ci on c.iCartId = ci.iCartId ");

            if (institutionId == 0)
            {
                sql.Append(" join tInstitution i on c.iInstitutionId = i.iInstitutionId and i.tiHouseAcct = 0");
            }

            sql.Append("    where c.tiRecordStatus = 1 ");

            if (institutionId > 0)
            {
                sql.Append(" and c.iInstitutionId = :institutionId ");
            }

            sql.Append("    and c.tiProcessed = 1 and ci.tiLicenseOriginalSourceId = 2 ")
                .Append("    and ci.dtCreationDate between :dateStart and :dateEnd ")
                .Append(") as [Purchased Pda] ").AppendLine();

            return sql.ToString();
        }

        private string GetIpAddressRangeFilterSql(IEnumerable<IpAddressRange> ipAddressRanges, string viewPrefix)
        {
            if (ipAddressRanges == null)
            {
                return null;
            }

            var sql = new StringBuilder();

            var or = "";
            var x = 0;
// ReSharper disable once UnusedVariable
            foreach (var range in ipAddressRanges)
            {
                sql.AppendFormat("{0}({1}.ipAddressInteger between :ipAddressStart_{2} and :ipAddressEnd_{2}) ", or,
                    viewPrefix, x);
                or = " or ";
                x++;
            }

            if (x > 0)
            {
                sql.Insert(0, " and (");
                sql.Append(")");
            }

            return sql.ToString();
        }

        private List<ResourceReportItem> ProcessResourceReportDataItems(IEnumerable<ResourceReportDataItem> dataItems,
            List<IResource> resources, ReportRequest reportRequest)
        {
            var reportItems = new List<ResourceReportItem>();
            ResourceReportItem reportItem = null;
            if (dataItems != null)
            {
                foreach (var dataItem in dataItems)
                {
                    if (reportItem == null || reportItem.ResourceId != dataItem.ResourceId)
                    {
                        var resource = resources.FirstOrDefault(x => x.Id == dataItem.ResourceId);
                        if (resource == null)
                        {
                            continue;
                        }

                        var newEditionResource = resource.LatestEditResourceId.HasValue
                            ? resources.FirstOrDefault(x => x.Id == resource.LatestEditResourceId)
                            : null;

                        reportItem = new ResourceReportItem
                        {
                            ResourceId = dataItem.ResourceId,
                            ResourceTitle = resource.Title,
                            ResourceSortTitle = resource.SortTitle,
                            ResourceIsbn = resource.Isbn,
                            ResourceImageName = resource.ImageFileName,
                            SessionCount = dataItem.SessionCount,
                            ContentRetrievalCount = dataItem.ContentRetrievalCount,
                            TocRetrievalCount = dataItem.TocRetrievalCount,
                            ConcurrencyTurnawayCount = dataItem.ConcurrentTurnawayCount,
                            AccessTurnawayCount = dataItem.AccessTurnawayCount,
                            ContentEmailCount = dataItem.EmailCount,
                            ContentPrintCount = dataItem.PrintCount,
                            ResourceStatusId = resource.StatusId,
                            Isbn10 = resource.Isbn10,
                            Isbn13 = resource.Isbn13,
                            EIsbn = resource.EIsbn,
                            Publisher = resource.Publisher.Name,
                            VendorNumber = resource.Publisher.VendorNumber,
                            Authors = resource.Authors,
                            Affiliation = resource.Affiliation,
                            NewEditionResourceIsbn = newEditionResource?.Isbn,
                            ResourceListPrice = resource.ListPrice,
                            TotalLicenseCount = dataItem.LicenseCount,
                            IsFreeResource = resource.IsFreeResource,
                            ReleaseDate = resource.ReleaseDate,
                            CopyRightYear = resource.PublicationDate?.Year,
                            PracticeAreaString = resource.PracticeAreasToString(),
                            SpecialtyString = resource.SpecialtiesToString()
                        };

                        if (resource.Collections != null && resource.Collections.Any())
                        {
                            //5   DCT
                            //6   DCT Essentials
                            var collectionItem = resource.Collections.FirstOrDefault(x => x.Id == 6);
                            if (collectionItem != null)
                            {
                                reportItem.DctStatus = collectionItem.Name;
                            }
                            else
                            {
                                collectionItem = resource.Collections.FirstOrDefault(x => x.Id == 5);
                                if (collectionItem != null)
                                {
                                    reportItem.DctStatus = collectionItem.Name;
                                }
                            }
                        }

                        if (!reportRequest.IsPublisherUser)
                        {
                            reportItem.FirstPurchasedDate = dataItem.FirstPurchaseDate;
                            reportItem.OriginalSource = dataItem.OriginalSource;
                            reportItem.PdaCreatedDate = dataItem.PdaCreatedDate;
                            reportItem.PdaAddedToCartDate = dataItem.PdaAddedToCartDate;
                            reportItem.ResourceAveragePrice = dataItem.ResourceAveragePrice;
                            reportItem.TotalPdaAccess = dataItem.PdaViews;
                        }

                        reportItems.Add(reportItem);
                    }
                }
            }

            _log.DebugFormat("reportItems.Count: {0}", reportItems.Count);

            // sort results
            switch (reportRequest.SortBy)
            {
                case ReportSortBy.AccessTurnaways:
                    reportItems = reportItems.OrderByDescending(x => x.AccessTurnawayCount)
                        .ThenBy(x => x.ResourceSortTitle).ToList();
                    break;
                case ReportSortBy.ContentRetrievals:
                    reportItems = reportItems.OrderByDescending(x => x.ContentRetrievalCount)
                        .ThenBy(x => x.ResourceSortTitle)
                        .ToList();
                    break;
                case ReportSortBy.SessionCount:
                    reportItems = reportItems.OrderByDescending(x => x.SessionCount).ThenBy(x => x.ResourceSortTitle)
                        .ToList();
                    break;
                case ReportSortBy.ContentTurnaways:
                    reportItems = reportItems.OrderByDescending(x => x.ConcurrencyTurnawayCount)
                        .ThenBy(x => x.ResourceSortTitle)
                        .ToList();
                    break;
                case ReportSortBy.FirstPurchaseDate:
                    reportItems = reportItems.OrderBy(x => x.FirstPurchasedDate).ThenBy(x => x.ResourceSortTitle)
                        .ToList();
                    break;
                case ReportSortBy.LicenseCount:
                    reportItems = reportItems.OrderByDescending(x => x.TotalLicenseCount)
                        .ThenBy(x => x.ResourceSortTitle).ToList();
                    break;
                case ReportSortBy.PdaViews:
                    reportItems = reportItems.OrderByDescending(x => x.TotalPdaAccess).ThenBy(x => x.ResourceSortTitle)
                        .ToList();
                    break;
                case ReportSortBy.Copyright:
                    reportItems = reportItems.OrderByDescending(x => x.CopyRightYear.HasValue)
                        .ThenByDescending(x => x.CopyRightYear)
                        .ThenBy(x => x.ResourceSortTitle).ToList();
                    break;
                case ReportSortBy.ReleaseDate:
                    reportItems = reportItems.OrderByDescending(x => x.ReleaseDate.HasValue)
                        .ThenByDescending(x => x.ReleaseDate)
                        .ThenBy(x => x.ResourceSortTitle).ToList();
                    break;
                default:
                    reportItems = reportItems.OrderBy(x => x.ResourceSortTitle).ToList();
                    break;
            }

            return reportItems;
        }


        private string GetAnnualFeeReportSql(ReportRequest reportRequest)
        {
            #region "Real Sql"

            //select i.iInstitutionId as 'InstitutionId', i.vchInstitutionAcctNum as 'AccountNumber', i.vchInstitutionName as 'InstitutionName'
            //, concat(Rtrim(u.vchFirstName), ' ', Ltrim(u.vchLastName)) as 'ContactName', u.vchUserEmail as 'ContactEmail', i.dtAnnualFee as 'ActiveDate'
            //, CONVERT(smalldatetime,concat(Month(i.dtAnnualFee), '/', Day(i.dtAnnualFee), '/', Year(getdate()))) as 'RenewalDate', u.iUserId as 'UserId'
            //from tInstitution i
            //join tUser u on i.iInstitutionId = u.iInstitutionId and ISNUMERIC(u.vchUserName) = 1 and u.iRoleId = 1 and u.tiRecordStatus = 1
            //where i.tiRecordStatus = 1 and dtAnnualFee is not null and i.tiHouseAcct = 0  and i.vchInstitutionAcctNum <> '999999'
            //and CONVERT(smalldatetime,concat(Month(i.dtAnnualFee), '/', Day(i.dtAnnualFee), '/',
            //Year('9/26/2014 11:12:36 AM'))) between  '9/25/2014 11:12:36 AM' and '9/26/2014 11:12:36 AM' and i.iInstitutionAcctStatusId = 1
            //and Year(i.dtAnnualFee) <> year(getdate())
            //union
            //select i.iInstitutionId as 'InstitutionId', i.vchInstitutionAcctNum as 'AccountNumber', i.vchInstitutionName as 'InstitutionName'
            //, concat(Rtrim(u.vchFirstName), ' ', Ltrim(u.vchLastName)) as 'ContactName', u.vchUserEmail as 'ContactEmail', i.dtAnnualFee as 'ActiveDate'
            //, CONVERT(smalldatetime,concat(Month(i.dtAnnualFee), '/', Day(i.dtAnnualFee), '/', Year(getdate()))) as 'RenewalDate', u.iUserId as 'UserId'
            //from tInstitution i
            //join tUser u on i.iInstitutionId = u.iInstitutionId and ISNUMERIC(u.vchUserName) = 1 and u.iRoleId = 1 and u.tiRecordStatus = 1
            //where i.tiRecordStatus = 1 and i.dtAnnualFee is not null and i.tiHouseAcct = 0  and i.vchInstitutionAcctNum <> '999999' and
            //CONVERT(smalldatetime,concat(Month(i.dtAnnualFee), '/', Day(i.dtAnnualFee), '/', Year('9/26/2014 11:12:36 AM')))
            //between  '9/26/2014 11:12:36 AM' and '9/26/2014 11:12:36 AM'  and i.iInstitutionAcctStatusId = 1 and Year(i.dtAnnualFee) <> year(getdate())
            //order by 7

            #endregion

            var sql = new StringBuilder()
                .Append(
                    " select i.iInstitutionId as 'InstitutionId', i.vchInstitutionAcctNum as 'AccountNumber', i.vchInstitutionName as 'InstitutionName' ")
                .Append(
                    " , concat(Rtrim(u.vchFirstName), ' ', Ltrim(u.vchLastName)) as 'ContactName', u.vchUserEmail as 'ContactEmail', i.dtAnnualFee as 'ActiveDate' ")
                .Append(
                    " , CONVERT(smalldatetime,concat(Month(i.dtAnnualFee), '/', Day(i.dtAnnualFee), '/', Year(getdate()))) as 'RenewalDate', u.iUserId as 'UserId' ")
                .Append(" , i.vchConsortia as 'Consortia' ")
                .Append(" from tInstitution i ")
                .Append(
                    " join tUser u on i.iInstitutionId = u.iInstitutionId and ISNUMERIC(u.vchUserName) = 1 and len(u.vchUserName) = 6 and u.iRoleId = 1 and u.tiRecordStatus = 1 ")
                .Append(
                    " where i.tiRecordStatus = 1 and dtAnnualFee is not null and i.tiHouseAcct = 0  and i.vchInstitutionAcctNum <> '999999' and ")
                .Append(" CONVERT(smalldatetime,concat(Month(i.dtAnnualFee), '/', Day(i.dtAnnualFee), '/', Year( ")
                .Append(" {0}")
                .Append(" ))) between ")
                .Append(" :dateStart and :dateEnd and i.iInstitutionAcctStatusId = 1 ")
                //Only get future Renewals if the end date is >= than now
                .AppendFormat("{0}",
                    reportRequest.DateRangeEnd >= DateTime.Now &&
                    reportRequest.DateRangeStart != reportRequest.DateRangeEnd
                        ? ""
                        : "and Year(i.dtAnnualFee) <> year(getdate())")
                .ToString();

            return new StringBuilder()
                .AppendFormat(sql, ":dateStart")
                .Append(" union ")
                .AppendFormat(sql, ":dateEnd")
                .Append(
                    " order by 7--CONVERT(smalldatetime,concat(Month(i.dtAnnualFee), '/', Day(i.dtAnnualFee), '/', Year(getdate())))")
                .ToString();
        }

        private string GetResourceUsageQuery(ReportRequest reportRequest)
        {
            var sql = new StringBuilder();
            sql.Append(" select * from ( ");
            sql.Append("select ResourceId ")
                .Append("    , sum(contentRetrievalCount) as ContentRetrievalCount ")
                .Append("    , sum(tocRetrievalCount) as TocRetrievalCount ")
                .Append("    , sum(sessionCount) as SessionCount ")
                .Append("    , sum(printCount) as PrintCount ")
                .Append("    , sum(emailCount) as EmailCount ")
                .Append("    , sum(accessTurnawayCount) as AccessTurnawayCount ")
                .Append("    , sum(concurrentTurnawayCount) as ConcurrentTurnawayCount ")
                .Append("    , case when max(case when FirstPurchaseDate is null then 0 else 1 end ) = 1 ")
                .Append("        then min(FirstPurchaseDate) end as FirstPurchaseDate ")
                .Append("    , case when max(case when PdaAddedToCartDate is null then 0 else 1 end ) = 1 ")
                .Append("        then min(PdaAddedToCartDate) end as PdaAddedToCartDate ")
                .Append("    , case when max(case when PdaCreatedDate is null then 0 else 1 end ) = 1 ")
                .Append("        then min(PdaCreatedDate) end as PdaCreatedDate ")
                .Append("    , max(OriginalSource) as OriginalSource ")
                .Append("    , sum(LicenseCount) as LicenseCount ")
                .Append("    , sum(PdaViews) as PdaViews ")
                .Append("    from ( ")
                .Append("    select dirsc.resourceId ")
                .Append("        , sum(contentRetrievalCount) as contentRetrievalCount ")
                .Append("        , sum(tocRetrievalCount) as tocRetrievalCount ")
                .Append("        , sum(sessionCount) as sessionCount ")
                .Append("        , sum(printCount) as printCount ")
                .Append("        , sum(emailCount) as emailCount ")
                .Append("        , sum(accessTurnawayCount) as accessTurnawayCount ")
                .Append("        , sum(concurrentTurnawayCount) as concurrentTurnawayCount ")
                .Append("        , null as FirstPurchaseDate ")
                .Append("        , null as PdaAddedToCartDate ")
                .Append("        , null as PdaCreatedDate ")
                .Append("        , 0 as OriginalSource ")
                .Append("        , 0 as LicenseCount ")
                .Append("        , 0 as PdaViews ")
                .Append("    from vDailyInstitutionResourceStatisticsCount dirsc ")
                .Append(
                    "        join tResource r on r.iResourceId = dirsc.resourceId and r.tiRecordStatus = 1 and r.iResourceStatusId <> 72 ");

            if (reportRequest.PracticeAreaId > 0)
            {
                sql.Append(
                    "        join tResourcePracticeArea rpa on rpa.iResourceId = r.iResourceId and rpa.iPracticeAreaId = :practiceAreaId and rpa.tiRecordStatus = 1 ");
            }

            if (reportRequest.SpecialtyId > 0)
            {
                sql.Append(
                    "        join tResourceSpecialty rs on rs.iResourceId = r.iResourceId and rs.iSpecialtyId = :specialtyId and rs.tiRecordStatus = 1 ");
            }

            if (reportRequest.PublisherId > 0)
            {
                sql.Append(
                    "        join tPublisher p on p.iPublisherId = r.iPublisherId and (p.iPublisherId = :publisherId or p.iConsolidatedPublisherId = :publisherId) and p.tiRecordStatus = 1 ");
            }

            if (reportRequest.InstitutionId == 0)
            {
                sql.Append(
                    "       join tInstitution i on dirsc.InstitutionId = i.iInstitutionId and i.tiHouseAcct = 0");

                if (reportRequest.InstitutionTypeId > 0)
                {
                    sql.Append($" and i.iInstitutionTypeId = {reportRequest.InstitutionTypeId}");
                }
            }

            sql.Append(
                    "        left join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and irl.iInstitutionId = dirsc.institutionId and irl.tiRecordStatus = 1 ")
                .Append("    where dirsc.institutionResourceStatisticsDate between :dateStart and :dateEnd ");

            if (reportRequest.InstitutionId > 0)
            {
                sql.Append("        and dirsc.institutionId = :institutionId ");
            }

            if (reportRequest.IsTrialAccount)
            {
                //DO Nothing
            }
            else if (!reportRequest.IncludeTrialStats)
            {
                sql.Append(" and dirsc.licenseType <> 2 ");
            }
            //This is needed if you want to look at TRIAL stats only and they are an active/pda account.
            else if (reportRequest.IncludeTrialStats && !reportRequest.IncludePdaTitles &&
                     !reportRequest.IncludePurchasedTitles &&
                     !reportRequest.IncludeTocTitles)
            {
                sql.Append(" and dirsc.licenseType = 2 ");
            }

            sql.Append(GetLicenseTypeSql(reportRequest));

            if (reportRequest.GetCondensedRanges() != null && reportRequest.GetCondensedRanges().Any())
            {
                sql.Append(" and ( ");
                sql.Append(GetIpRangeSql(reportRequest));
                sql.Append(" ) ");
            }

            sql.Append("    group by dirsc.resourceId ")
                .Append("union ")
                .Append("    select r.iResourceId as resourceId ")
                .Append("        , 0 as contentRetrievalCount ")
                .Append("        , 0 as tocRetrievalCount ")
                .Append("        , 0 as sessionCount ")
                .Append("        , 0 as printCount ")
                .Append("        , 0 as emailCount ")
                .Append("        , 0 as accessTurnawayCount ")
                .Append("        , 0 as concurrentTurnawayCount ")
                .Append("        , min(irl.dtFirstPurchaseDate) as FirstPurchaseDate ")
                .Append("        , min(irl.dtPdaAddedToCartDate) as PdaAddedToCartDate ")
                .Append("        , min(irl.dtPdaAddedDate) as PdaCreatedDate ")
                .Append("        , max(irl.tiLicenseOriginalSourceId) as OriginalSource ")
                .Append("        , sum(irl.iLicenseCount) as LicenseCount ")
                .Append("        , isnull(sum(irl.iPdaViewCount), 0) as PdaViews ")
                .Append("    from tResource r ");

            if (reportRequest.PracticeAreaId > 0)
            {
                // sjs - 10/16/2015 - added check to make sure the tResourcePracticeArea record has not been soft deleted
                sql.Append(
                    "        join tResourcePracticeArea rpa on rpa.iResourceId = r.iResourceId and rpa.iPracticeAreaId = :practiceAreaId and rpa.tiRecordStatus = 1 ");
            }

            if (reportRequest.SpecialtyId > 0)
            {
                sql.Append(
                    "        join tResourceSpecialty rs on rs.iResourceId = r.iResourceId and rs.iSpecialtyId = :specialtyId and rs.tiRecordStatus = 1 ");
            }

            if (reportRequest.PublisherId > 0)
            {
                sql.Append(
                    "        join tPublisher p on p.iPublisherId = r.iPublisherId and (p.iPublisherId = :publisherId or p.iConsolidatedPublisherId = :publisherId) and p.tiRecordStatus = 1 ");
            }

            sql.Append(
                "        join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and irl.tiRecordStatus = 1 ");

            if (reportRequest.InstitutionId == 0)
            {
                sql.Append("       join tInstitution i on irl.iInstitutionId = i.iInstitutionId and i.tiHouseAcct = 0");
                if (reportRequest.InstitutionTypeId > 0)
                {
                    sql.Append($" and i.iInstitutionTypeId = {reportRequest.InstitutionTypeId}");
                }
            }

            sql.Append("    where r.tiRecordStatus = 1 and r.iResourceStatusId <> 72 ");

            if (reportRequest.InstitutionId > 0)
            {
                sql.Append("        and irl.iInstitutionId = :institutionId ");
            }

            sql.Append(GetLicenseTypeSql(reportRequest));

            sql.Append("    group by r.iResourceId ");

            if (reportRequest.IncludeTocTitles || reportRequest.IsTrialAccount)
            {
                sql.Append("union ")
                    .Append("    select r.iResourceId as resourceId ")
                    .Append("        , 0 as contentRetrievalCount ")
                    .Append("        , 0 as tocRetrievalCount ")
                    .Append("        , 0 as sessionCount ")
                    .Append("        , 0 as printCount ")
                    .Append("        , 0 as emailCount ")
                    .Append("        , 0 as accessTurnawayCount ")
                    .Append("        , 0 as concurrentTurnawayCount ")
                    .Append("        , null as FirstPurchaseDate ")
                    .Append("        , null as PdaAddedToCartDate ")
                    .Append("        , null as PdaCreatedDate ")
                    .Append("        , null as OriginalSource ")
                    .Append("        , 0 as LicenseCount ")
                    .Append("        , 0 as PdaViews ")
                    .Append("    from tResource r ");

                if (reportRequest.PublisherId > 0)
                {
                    sql.Append(
                        "        join tPublisher p on p.iPublisherId = r.iPublisherId and (p.iPublisherId = :publisherId or p.iConsolidatedPublisherId = :publisherId) and p.tiRecordStatus = 1 ");
                }

                if (reportRequest.PracticeAreaId > 0)
                {
                    sql.Append(
                        "        join tResourcePracticeArea rpa on rpa.iResourceId = r.iResourceId and rpa.iPracticeAreaId = :practiceAreaId and rpa.tiRecordStatus = 1 ");
                }

                if (reportRequest.SpecialtyId > 0)
                {
                    sql.Append(
                        "        join tResourceSpecialty rs on rs.iResourceId = r.iResourceId and rs.iSpecialtyId = :specialtyId and rs.tiRecordStatus = 1 ");
                }

                sql.Append("    group by r.iResourceId ");
            }

            sql.Append(") r ");

            if (reportRequest.ResourceId > 0)
            {
                sql.Append(" where r.ResourceId = :resourceId ");
            }

            sql.Append(" group by r.resourceId ");

            sql.Append(" ) rr ");

            return sql.ToString();
        }

        private static string GetLicenseTypeSql(ReportRequest reportRequest)
        {
            if (reportRequest.IsTrialAccount)
            {
                return "";
            }

            var pdaTitlesOnly = reportRequest.IncludePdaTitles && !reportRequest.IncludePurchasedTitles &&
                                !reportRequest.IncludeTocTitles;
            var tocTitlesOnly = reportRequest.IncludeTocTitles && !reportRequest.IncludePurchasedTitles &&
                                !reportRequest.IncludePdaTitles;
            var purchasedTitlesOnly = reportRequest.IncludePurchasedTitles && !reportRequest.IncludePdaTitles &&
                                      !reportRequest.IncludeTocTitles;

            var pdaAndTocTitles = reportRequest.IncludePdaTitles && reportRequest.IncludeTocTitles &&
                                  !reportRequest.IncludePurchasedTitles;
            var pdaAndPurchasedTitles = reportRequest.IncludePdaTitles && reportRequest.IncludePurchasedTitles &&
                                        !reportRequest.IncludeTocTitles;
            var purchasedAndTocTitles = reportRequest.IncludeTocTitles && reportRequest.IncludePurchasedTitles &&
                                        !reportRequest.IncludePdaTitles;

            var singleSelection = pdaTitlesOnly || tocTitlesOnly || purchasedTitlesOnly;
            var doubleSelection = pdaAndTocTitles || pdaAndPurchasedTitles || purchasedAndTocTitles;

            var sql = new StringBuilder();

            if (singleSelection)
            {
                if (purchasedTitlesOnly)
                {
                    sql.Append("        and irl.tiLicenseTypeId = 1 ");
                }
                else if (pdaTitlesOnly)
                {
                    sql.Append("        and irl.tiLicenseTypeId = 3 ");
                }
                else // TOC Only
                {
                    sql.Append("        and irl.iInstitutionResourceLicenseId is null ");
                }
            }
            else if (doubleSelection)
            {
                if (pdaAndTocTitles)
                {
                    sql.Append("        and (irl.iInstitutionResourceLicenseId is null or irl.tiLicenseTypeId = 3) ");
                }
                else if (pdaAndPurchasedTitles)
                {
                    sql.Append("        and irl.tiLicenseTypeId in (1,3) ");
                }
                else // purchasedAndTocTitles
                {
                    sql.Append("        and (irl.iInstitutionResourceLicenseId is null or irl.tiLicenseTypeId = 1) ");
                }
            }

            return sql.ToString();
        }

        private string GetIpRangeSql(ReportRequest reportRequest)
        {
            var sql = new StringBuilder();
            var or = "";

            foreach (var ipAddressRange in reportRequest.GetCondensedRanges())
            {
                sql.AppendFormat("{0}(dirsc.ipAddressInteger between {1} and {2}) ", or, ipAddressRange.IpNumberStart,
                    ipAddressRange.IpNumberEnd);
                or = " or ";
            }

            return sql.ToString();
        }

        private int ConvertObjectToInt(object value)
        {
            if (value == null)
            {
                return 0;
            }

            return (int)value;
        }

        private decimal ConvertObjectToDecimal(object value)
        {
            if (value == null)
            {
                return 0;
            }

            return (decimal)value;
        }

        public string GetYesturdayTurnaways(string reportDatabaseName, string r2DatabaseName)
        {
            var date = DateTime.Now.AddDays(-1).ToShortDateString();

            var startTime = DateTime.Parse($"{date} 00:00:01");
            var endTime = DateTime.Parse($"{date} 23:59:59");

            return $@"
Select cv.ResourceId, cv.contentViewTimestamp as TurnawayTimeStamp
,	CAST(case
		when cv.turnawayTypeId = 21 then 1
		when cv.turnawayTypeId = 20 then 0
	end AS BIT) as IsAccessTurnaway
, cv.RequestId, pv.SessionId
, concat(pv.ipAddressOctetA, '.', pv.ipAddressOctetB, '.', pv.ipAddressOctetC, '.', pv.ipAddressOctetD) as IpAddress
, cv.InstitutionId
from {reportDatabaseName}..ContentView cv
join {reportDatabaseName}..PageView pv on cv.requestId = pv.requestId and cv.InstitutionId = pv.InstitutionId
join {r2DatabaseName}..tResource r on cv.resourceId = r.iResourceId and r.iResourceStatusId = 6 and r.dtNotSaleableDate is null
where
(cv.contentViewTimestamp > '{startTime}' and cv.contentViewTimestamp < '{endTime}')
and cv.turnawayTypeId <> 0
and cv.InstitutionId > 0
order by 7
";
        }

        public bool IsValidDate(DateTime date)
        {
            if (date == DateTime.MinValue)
            {
                return false;
            }

            if (date == DateTime.MaxValue)
            {
                return false;
            }

            if (date < new DateTime(2004, 1, 1)) //Rittenhousse Create Date 2004-10-20 00:00:00
            {
                return false;
            }

            if (date > DateTime.Now.AddYears(10)) //Rittenhousse Create Date 2004-10-20 00:00:00
            {
                return false;
            }

            return true;
        }
    }
}

public class ResourceRequestItem
{
    public int InstitutionId { get; set; }
    public int ResourceId { get; set; }
    public int RequestCount { get; set; }
    public DateTime LastRequestDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public int? OrderHistoryId { get; set; }
    public int? CartId { get; set; }
    public int? AutomatedCartId { get; set; }
    public string AutomatedCartName { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? AddedDate { get; set; }
    public int? InstitutionTypeId { get; set; }
    public int? TerritoryId { get; set; }

    public string ToDebug()
    {
        return $@"
InstitutionId: {InstitutionId}
ResourceId: {ResourceId}
RequestCount: {RequestCount}
LastRequestDate: {LastRequestDate}
PurchasePrice: {PurchasePrice}
OrderHistoryId: {OrderHistoryId}
CartId: {CartId}
AutomatedCartId: {AutomatedCartId}
AutomatedCartName: {AutomatedCartName}
PurchaseDate: {PurchaseDate}
AddedDate: {AddedDate}
";
    }
}