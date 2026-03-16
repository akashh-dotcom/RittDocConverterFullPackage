#region

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Reports;

#endregion

namespace R2Utilities.DataAccess
{
    public class UtilitiesStatisticsService : R2ReportsBase
    {
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly ReportDataService _reportDataService;

        public UtilitiesStatisticsService(IR2UtilitiesSettings r2UtilitiesSettings, ReportDataService reportDataService)
        {
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _reportDataService = reportDataService;
        }

        public bool InsertInstitutionStatistics(InstitutionStatistics institutionStatistics)
        {
            var sql = new StringBuilder()
                .AppendFormat("INSERT INTO InstitutionMonthlyStatisticsCount ")
                .Append("           ([institutionId] ")
                .Append("           ,[aggregationDate] ")
                .Append("           ,[mostAccessedResourceId] ")
                .Append("           ,[mostAccessedCount] ")
                .Append("           ,[leastAccessedResourceId] ")
                .Append("           ,[leastAccessedCount] ")
                .Append("           ,[mostTurnawayConcurrentResourceId] ")
                .Append("           ,[mostTurnawayConcurrentCount] ")
                .Append("           ,[mostTurnawayAccessResourceId] ")
                .Append("           ,[mostTurnawayAccessCount] ")
                .Append("           ,[mostPopularSpecialtyName] ")
                .Append("           ,[mostPopularSpecialtyCount] ")
                .Append("           ,[leastPopularSpecialtyName] ")
                .Append("           ,[leastPopularSpecialtyCount] ")
                .Append("           ,[totalResourceCount] ")
                .Append("           ,[contentCount] ")
                .Append("           ,[tocCount] ")
                .Append("           ,[sessionCount] ")
                .Append("           ,[printCount] ")
                .Append("           ,[emailCount] ")
                .Append("           ,[turnawayConcurrencyCount] ")
                .Append("           ,[turnawayAccessCount]) ")
                .Append("            VALUES (")
                .AppendFormat("                {0},", institutionStatistics.InstitutionId)
                .AppendFormat("                  '{0}',", institutionStatistics.StartDate)
                .AppendFormat("                    {0},", institutionStatistics.Highlights.MostAccessedResourceId)
                .AppendFormat("                    {0},", institutionStatistics.Highlights.MostAccessedCount)
                .AppendFormat("                    {0},", institutionStatistics.Highlights.LeastAccessedResourceId)
                .AppendFormat("                    {0},", institutionStatistics.Highlights.LeastAccessedCount)
                .AppendFormat("                    {0},",
                    institutionStatistics.Highlights.MostTurnawayConcurrentResourceId)
                .AppendFormat("                    {0},", institutionStatistics.Highlights.MostTurnawayConcurrentCount)
                .AppendFormat("                    {0},", institutionStatistics.Highlights.MostTurnawayAccessResourceId)
                .AppendFormat("                    {0},", institutionStatistics.Highlights.MostTurnawayAccessCount)
                .AppendFormat("                  '{0}',", institutionStatistics.Highlights.MostPopularSpecialtyName)
                .AppendFormat("                    {0},", institutionStatistics.Highlights.MostPopularSpecialtyCount)
                .AppendFormat("                  '{0}',", institutionStatistics.Highlights.LeastPopularSpecialtyName)
                .AppendFormat("                    {0},", institutionStatistics.Highlights.LeastPopularSpecialtyCount)
                .AppendFormat("                    {0},", institutionStatistics.Highlights.TotalResourceCount)
                .AppendFormat("                    {0},", institutionStatistics.AccountUsage.ContentCount)
                .AppendFormat("                    {0},", institutionStatistics.AccountUsage.TocCount)
                .AppendFormat("                    {0},", institutionStatistics.AccountUsage.SessionCount)
                .AppendFormat("                    {0},", institutionStatistics.AccountUsage.PrintCount)
                .AppendFormat("                    {0},", institutionStatistics.AccountUsage.EmailCount)
                .AppendFormat("                    {0},", institutionStatistics.AccountUsage.TurnawayConcurrencyCount)
                .AppendFormat("                    {0})", institutionStatistics.AccountUsage.TurnawayAccessCount)
                .ToString();
            var count = ExecuteInsertStatementReturnRowCount(sql, null, false);
            return count > 0;
        }

        public int InsertMonthlyResourceStatistics(InstitutionStatistics institutionStatistics)
        {
            const string sql = @"
Insert into InstitutionMonthlyResourceStatistics (institutionId, aggregationDate, resourceId, purchased, archivedPurchased
, newEditionPreviousPurchased, pdaAdded, pdaAddedToCart, pdaNewEdition, expertRecommended)
select @InstitutionId, @StartDate, * from 
(
    select r.iResourceId
    ,case when (irl.tiLicenseTypeId = 1  and irl.dtFirstPurchaseDate between @StartDate and @EndDate)				then 1 else 0 end as Purchased
    ,case when (irl.tiLicenseTypeId = 1  and re.dateArchivedEmail between @StartDate and @EndDate)					then 1 else 0 end as ArchivedPurchased
    ,case when (irl2.iInstitutionResourceLicenseId > 0 and irl.dtFirstPurchaseDate between @StartDate and @EndDate)	then 1 else 0 end as NewEditionPreviousPurchased
    ,case when (irl.tiLicenseTypeId = 3 and irl.dtPdaAddedDate between @StartDate and @EndDate)						then 1 else 0 end as PdaAdded
    ,case when (irl.tiLicenseTypeId = 3 and irl.dtPdaAddedToCartDate between @StartDate and @EndDate)				then 1 else 0 end as PdaAddedToCart
    ,case when (re3.dateNewResourceEmail between @StartDate and @EndDate)											then 1 else 0 end as PdaNewEdition
    ,case when (ir.dtCreationDate between @StartDate and @EndDate)													then 1 else 0 end as ExpertRecommended
    from {0}..tInstitutionResourceLicense irl 
    join {0}..tResource r on irl.iResourceId = r.iResourceId
    left join {1}..ResourceEmails re on r.vchIsbn10 = re.resourceISBN
    left join {0}..tResource r2 on r.iPrevEditResourceID = r2.iResourceId
    left join {0}..tInstitutionResourceLicense irl2 on r2.iResourceId = irl2.iResourceId and irl.iInstitutionId = irl2.iInstitutionId
    left join {0}..tResource r3 on irl.iResourceId = r3.iPrevEditResourceID and irl.tiLicenseTypeId = 3
    left join {1}..ResourceEmails re3 on  r3.vchIsbn10 = re3.resourceISBN
    left join {0}..tInstitutionRecommendation ir on r.iResourceId = ir.iResourceId and ir.iInstitutionId = @InstitutionId
				and ir.tiRecordStatus = 1 and ir.dtDeletedDate is null
    where irl.iInstitutionId = @InstitutionId and irl.tiRecordStatus = 1
) t
where t.Purchased > 0 or t.ArchivedPurchased > 0 or NewEditionPreviousPurchased > 0 or PdaAdded > 0 or PdaAddedToCart > 0 or PdaNewEdition >  0 or ExpertRecommended > 0
order by iResourceId
";
            var sqlStatement = string.Format(sql, _r2UtilitiesSettings.R2DatabaseName,
                _r2UtilitiesSettings.R2UtilitiesDatabaseName);
            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("InstitutionId", institutionStatistics.InstitutionId),
                new DateTimeParameter("StartDate", institutionStatistics.StartDate),
                new DateTimeParameter("EndDate", institutionStatistics.StartDate.AddMonths(1).AddSeconds(-1))
            };

            SqlConnection cnn = null;
            SqlCommand command = null;

            try
            {
                cnn = GetConnection(ConnectionString);

                //This is set to be exactly like SSMS. ADO.NET turns this off by default. This results in different execution plan thatn SSMS. 
                command = new SqlCommand("SET ARITHABORT ON", cnn);
                command.ExecuteNonQuery();


                command = GetSqlCommand(cnn, sqlStatement, parameters, 300, null);

                var rows = command.ExecuteNonQuery();

                return rows;
            }
            catch (Exception ex)
            {
                LogCommandInfo(command);
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                DisposeConnections(cnn, command);
            }
        }

        public int AggregateInstitutionResourceStatisticsCount(DateTime startDate, DateTime endDate)
        {
            Log.InfoFormat("AggregateInstitutionResourceStatisticsCount(StartDate: {0} - EndDate: {1})", startDate,
                endDate);

            #region SQL

            const string sql = @"
INSERT INTO DailyInstitutionResourceStatisticsCount
           ([institutionId]
           ,[resourceId]
           ,[ipAddressInteger]
           ,[institutionResourceStatisticsDate]
           ,[licenseType]
           ,[resourceStatusId]
           ,[contentRetrievalCount]
           ,[tocRetrievalCount]
           ,[sessionCount]
           ,[printCount]
           ,[emailCount]
           ,[accessTurnawayCount]
           ,[concurrentTurnawayCount])
 select agg.institutionId, agg.resourceId, agg.ipAddressInteger, agg.institutionResourceStatisticsDate, agg.licenseType, agg.resourceStatusId
, sum(agg.contentCount) as contentCount
, sum(agg.tocCount) as tocCount
, sum(agg.sessionCount) as sessionCount
, sum(agg.printCount) as printCount
, sum(agg.emailCount) as emailCount
, sum(agg.accessCount) as accessCount
, sum(agg.concurrencyCount) as concurrencyCount
from
{0}
left join DailyInstitutionResourceStatisticsCount dirsc on 
agg.institutionId = dirsc.institutionId
and agg.resourceId = dirsc.resourceId
and agg.ipAddressInteger = dirsc.ipAddressInteger
and agg.institutionResourceStatisticsDate = dirsc.institutionResourceStatisticsDate
and agg.licenseType = dirsc.licenseType
and agg.resourceStatusId = dirsc.resourceStatusId

where dirsc.dailyInstitutionResourceStatisticsCountId is null

group by agg.institutionId, agg.resourceId, agg.ipAddressInteger, agg.institutionResourceStatisticsDate, agg.licenseType, agg.resourceStatusId
order by 4, 1, 2
";

            #endregion

            var useNewerQuery = startDate > DateTime.Now.AddMonths(-6);

            var sqlStatement = string.Format(sql,
                useNewerQuery
                    ? GetInstitutionResourceStatisticsBaseWithinSixMonthsQuery(startDate, endDate)
                    : GetInstitutionResourceStatisticsBaseBeforeSixMonthsQuery(startDate, endDate));

            SqlConnection cnn = null;
            SqlCommand command = null;

            try
            {
                cnn = GetConnection(ConnectionString);
                command = cnn.CreateCommand();
                command.CommandTimeout = _r2UtilitiesSettings.AggregateDailyCommandTimeout;

                command.CommandText = sqlStatement;
                var rows = command.ExecuteNonQuery();

                if (rows == 0)
                {
                    sqlStatement = string.Format(sql,
                        useNewerQuery
                            ? GetInstitutionResourceStatisticsBaseBeforeSixMonthsQuery(startDate, endDate)
                            : GetInstitutionResourceStatisticsBaseWithinSixMonthsQuery(startDate, endDate));

                    command.CommandText = sqlStatement;
                    rows = command.ExecuteNonQuery();
                }

                return rows;
            }
            catch (Exception ex)
            {
                LogCommandInfo(command);
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                DisposeConnections(cnn, command);
            }
        }

        public int UpdateInstitutionResourceStatisticsCount(DateTime startDate, DateTime endDate)
        {
            #region SQL

            var sql = @"
update dirsc
set contentRetrievalCount = agg2.contentCount
, tocRetrievalCount = agg2.tocCount
, sessionCount = agg2.sessionCount
, printCount = agg2.printCount
, emailCount = agg2.emailCount
, accessTurnawayCount = agg2.accessCount
, concurrentTurnawayCount = agg2.concurrencyCount
FROM            DailyInstitutionResourceStatisticsCount AS dirsc
JOIN (

SELECT        agg.institutionId, agg.resourceId, agg.ipAddressInteger, agg.institutionResourceStatisticsDate, SUM(agg.contentCount) AS contentCount, SUM(agg.tocCount) 
                         AS tocCount, SUM(agg.sessionCount) AS sessionCount, SUM(agg.printCount) AS printCount, SUM(agg.emailCount) AS emailCount, SUM(agg.accessCount) 
                         AS accessCount, SUM(agg.concurrencyCount) AS concurrencyCount
						 from

{0}
join DailyInstitutionResourceStatisticsCount AS dirsc 
ON  agg.institutionId = dirsc.institutionId 
AND agg.resourceId = dirsc.resourceId 
AND agg.ipAddressInteger = dirsc.ipAddressInteger 
AND agg.institutionResourceStatisticsDate = dirsc.institutionResourceStatisticsDate
GROUP BY 
agg.institutionId, agg.resourceId, agg.ipAddressInteger, agg.institutionResourceStatisticsDate
, dirsc.accessTurnawayCount
, dirsc.concurrentTurnawayCount
, dirsc.contentRetrievalCount
, dirsc.emailCount
, dirsc.printCount
, dirsc.sessionCount
, dirsc.tocRetrievalCount
having 
sum(agg.accessCount) <> dirsc.accessTurnawayCount
or sum(agg.concurrencyCount) <> dirsc.concurrentTurnawayCount
or sum(agg.contentCount) <> dirsc.contentRetrievalCount
or sum(agg.emailCount) <> dirsc.emailCount
or sum(agg.printCount) <> dirsc.printCount
or sum(agg.sessionCount) <> dirsc.sessionCount
or sum(agg.tocCount) <> dirsc.tocRetrievalCount

)  AS agg2 ON agg2.institutionId = dirsc.institutionId 
AND agg2.resourceId = dirsc.resourceId 
AND agg2.ipAddressInteger = dirsc.ipAddressInteger 
AND agg2.institutionResourceStatisticsDate = dirsc.institutionResourceStatisticsDate
";

            #endregion

            var useNewerQuery = startDate >= DateTime.Now.AddMonths(-6);
            var sqlStatement = string.Format(sql,
                useNewerQuery
                    ? GetInstitutionResourceStatisticsBaseWithinSixMonthsQuery(startDate, endDate)
                    : GetInstitutionResourceStatisticsBaseBeforeSixMonthsQuery(startDate, endDate));

            SqlConnection cnn = null;
            SqlCommand command = null;

            try
            {
                cnn = GetConnection(ConnectionString);
                command = cnn.CreateCommand();
                command.CommandText = sqlStatement;
                var rows = command.ExecuteNonQuery();

                if (rows == 0)
                {
                    sqlStatement = string.Format(sql,
                        useNewerQuery
                            ? GetInstitutionResourceStatisticsBaseBeforeSixMonthsQuery(startDate, endDate)
                            : GetInstitutionResourceStatisticsBaseWithinSixMonthsQuery(startDate, endDate));

                    command.CommandText = sqlStatement;
                    rows = command.ExecuteNonQuery();
                }

                return rows;
            }
            catch (Exception ex)
            {
                LogCommandInfo(command);
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                DisposeConnections(cnn, command);
            }
        }

        private string GetInstitutionResourceStatisticsBaseWithinSixMonthsQuery(DateTime startDate, DateTime endDate)
        {
            #region sql

            var sql = @"

 (SELECT        institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, cast(cv.contentViewTimestamp as date) AS institutionResourceStatisticsDate, count(institutionId) 
							AS concurrencyCount, 0 AS accessCount, 0 AS sessionCount, 0 AS tocCount, 0 AS contentCount, 0 AS printCount, 0 AS emailCount
  FROM            ContentView cv 
  WHERE        (turnawayTypeId = 20) AND (institutionId > 0) AND (contentViewTimestamp >= '{0}' AND contentViewTimestamp < '{1}')
  GROUP BY institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, cast(contentViewTimestamp as date)
  UNION ALL
  SELECT        institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, cast(cv.contentViewTimestamp as date) AS institutionResourceStatisticsDate, 0 AS concurrencyCount, 
						   count(institutionId) AS accessCount, 0 AS sessionCount, 0 AS tocCount, 0 AS contentCount, 0 AS printCount, 0 AS emailCount
  FROM            ContentView cv 
  WHERE        (turnawayTypeId = 21) AND (institutionId > 0) AND (contentViewTimestamp >= '{0}' AND contentViewTimestamp < '{1}')
  GROUP BY institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, cast(cv.contentViewTimestamp as date)
  UNION ALL
  SELECT        pv.institutionId, resourceId, pv.ipAddressInteger, licenseType, resourceStatusId, cast(pv.pageViewTimestamp as date) AS institutionResourceStatisticsDate, 0 AS concurrencyCount, 0 AS accessCount, 
	count(distinct pv.sessionId) AS sessionCount, 0 AS tocCount, 0 AS contentCount, 0 AS printCount, 0 AS emailCount
  FROM            PageView pv
  join		      ContentView cv on pv.requestId = cv.requestId 
  WHERE        ( pv.institutionId > 0) AND (pageViewTimestamp >= '{0}' AND pageViewTimestamp < '{1}')
  GROUP BY  pv.institutionId, resourceId, pv.ipAddressInteger, licenseType, resourceStatusId, cast(pv.pageViewTimestamp as date)
  UNION ALL
  SELECT        institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, cast(cv.contentViewTimestamp as date) AS institutionResourceStatisticsDate, 0 AS concurrencyCount, 0 AS accessCount, 
						   0 AS sessionCount, count(institutionId) AS tocCount, 0 AS contentCount, 0 AS printCount, 0 AS emailCount
  FROM            ContentView cv
  WHERE        (institutionId > 0) AND (contentViewTimestamp >= '{0}' AND contentViewTimestamp < '{1}') AND (chapterSectionId IS NULL) 
				AND (actionTypeId = 0) and turnawayTypeId = 0
  GROUP BY institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, cast(cv.contentViewTimestamp as date)
  UNION ALL
  SELECT        cv.institutionId, cv.resourceId, cv.ipAddressInteger, licenseType, resourceStatusId, cast(cv.contentViewTimestamp as date) AS institutionResourceStatisticsDate, 0 AS concurrencyCount, 
						   0 AS accessCount, 0 AS sessionCount, 0 AS tocCount, count(cv.institutionId) AS contentCount, 0 AS printCount, 0 AS emailCount
  FROM            ContentView cv 
  WHERE        (cv.institutionId > 0) AND (contentViewTimestamp >= '{0}' AND contentViewTimestamp < '{1}') AND (cv.chapterSectionId IS NOT NULL) 
					AND (cv.actionTypeId = 0) and turnawayTypeId = 0
  GROUP BY cv.institutionId, cv.resourceId, cv.ipAddressInteger, licenseType, resourceStatusId, cast(cv.contentViewTimestamp as date)
  UNION ALL
  SELECT        institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, cast(cv.contentViewTimestamp as date) AS institutionResourceStatisticsDate, 0 AS concurrencyCount, 0 AS accessCount, 
						   0 AS sessionCount, 0 AS tocCount, 0 AS contentCount, count(institutionId) AS printCount, 0 AS emailCount
  FROM            ContentView cv
  WHERE        (institutionId > 0) AND (contentViewTimestamp >= '{0}' AND contentViewTimestamp < '{1}') AND (actionTypeId = 16)
				 and turnawayTypeId = 0
  GROUP BY institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, cast(cv.contentViewTimestamp as date)
  UNION ALL
  SELECT        institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, cast(cv.contentViewTimestamp as date) AS institutionResourceStatisticsDate, 0 AS concurrencyCount, 0 AS accessCount, 
						   0 AS sessionCount, 0 AS tocCount, 0 AS contentCount, 0 AS printCount, count(institutionId) AS emailCount
  FROM            ContentView cv
  WHERE        (institutionId > 0) AND (contentViewTimestamp >= '{0}' AND contentViewTimestamp < '{1}') AND (actionTypeId = 17)
				 and turnawayTypeId = 0
  GROUP BY institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, cast(cv.contentViewTimestamp as date)) AS agg 
";

            #endregion

            return string.Format(sql, startDate, endDate);
        }

        private string GetInstitutionResourceStatisticsBaseBeforeSixMonthsQuery(DateTime startDate, DateTime endDate)
        {
            #region sql

            var sql = @"
 (SELECT        institutionId, resourceId, ipAddressInteger, 0 as licenseType, 0 as resourceStatusId, contentTurnawayDate AS institutionResourceStatisticsDate, sum(dctc.contentTurnawayCount) 
							AS concurrencyCount, 0 AS accessCount, 0 AS sessionCount, 0 AS tocCount, 0 AS contentCount, 0 AS printCount, 0 AS emailCount
  FROM            DailyContentTurnawayCount dctc 
  WHERE        (turnawayTypeId = 20) AND (institutionId > 0) AND (contentTurnawayDate = '{0}')
  GROUP BY institutionId, resourceId, ipAddressInteger, contentTurnawayDate
  UNION ALL
  SELECT        institutionId, resourceId, ipAddressInteger, 0 as licenseType, 0 as resourceStatusId, contentTurnawayDate AS institutionResourceStatisticsDate, 0 AS concurrencyCount, 
						   sum(contentTurnawayCount) AS accessCount, 0 AS sessionCount, 0 AS tocCount, 0 AS contentCount, 0 AS printCount, 0 AS emailCount
  FROM            DailyContentTurnawayCount
  WHERE        (turnawayTypeId = 21) AND (institutionId > 0) AND (contentTurnawayDate = '{0}' )
  GROUP BY institutionId, resourceId, ipAddressInteger, contentTurnawayDate
  UNION ALL
  SELECT        institutionId, resourceId, ipAddressInteger, 0 as licenseType, 0 as resourceStatusId, sessionDate AS institutionResourceStatisticsDate, 0 AS concurrencyCount, 0 AS accessCount, 
	sum(sessionCount) AS sessionCount, 0 AS tocCount, 0 AS contentCount, 0 AS printCount, 0 AS emailCount
  FROM            DailyResourceSessionCount
  WHERE        ( institutionId > 0) AND (sessionDate = '{0}')
  GROUP BY  institutionId, resourceId, ipAddressInteger, sessionDate
  UNION ALL
  SELECT        institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, contentViewDate AS institutionResourceStatisticsDate, 0 AS concurrencyCount, 0 AS accessCount, 
						   0 AS sessionCount, sum(contentViewCount) AS tocCount, 0 AS contentCount, 0 AS printCount, 0 AS emailCount
  FROM            DailyContentViewCount
  WHERE        (institutionId > 0) AND (contentViewDate = '{0}') AND (chapterSectionId IS NULL) 
				AND (actionTypeId = 0)
  GROUP BY institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, contentViewDate
  UNION ALL
  SELECT        institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, contentViewDate AS institutionResourceStatisticsDate, 0 AS concurrencyCount, 
						   0 AS accessCount, 0 AS sessionCount, 0 AS tocCount, sum(contentViewCount) AS contentCount, 0 AS printCount, 0 AS emailCount
  FROM            DailyContentViewCount 
  WHERE        (institutionId > 0) AND (contentViewDate = '{0}') AND (chapterSectionId IS NOT NULL) 
					AND (actionTypeId = 0)
  GROUP BY institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, contentViewDate
  UNION ALL
  SELECT        institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, contentViewDate AS institutionResourceStatisticsDate, 0 AS concurrencyCount, 0 AS accessCount, 
						   0 AS sessionCount, 0 AS tocCount, 0 AS contentCount, sum(contentViewCount) AS printCount, 0 AS emailCount
  FROM            DailyContentViewCount
  WHERE        (institutionId > 0) AND (contentViewDate = '{0}') AND (actionTypeId = 16)
  GROUP BY institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, contentViewDate
  UNION ALL
  SELECT        institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, contentViewDate AS institutionResourceStatisticsDate, 0 AS concurrencyCount, 0 AS accessCount, 
						   0 AS sessionCount, 0 AS tocCount, 0 AS contentCount, 0 AS printCount, count(institutionId) AS emailCount
  FROM            DailyContentViewCount
  WHERE        (institutionId > 0) AND (contentViewDate = '{0}') AND (actionTypeId = 17)
  GROUP BY institutionId, resourceId, ipAddressInteger, licenseType, resourceStatusId, contentViewDate) AS agg 
";

            #endregion

            return string.Format(sql, startDate.ToString("d"));
        }

        public void RebuildAndReorgIndexes()
        {
            _reportDataService.RebuildAndReorgIndexes(_r2UtilitiesSettings.R2ReportsDatabaseName);
        }
    }
}