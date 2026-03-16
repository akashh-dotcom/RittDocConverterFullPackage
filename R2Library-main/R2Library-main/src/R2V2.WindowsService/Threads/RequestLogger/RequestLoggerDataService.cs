#region

using System;
using System.Collections.Generic;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2V2.Core.RequestLogger;
using R2V2.Infrastructure.Logging;
using R2V2.WindowsService.DataServices;

#endregion

namespace R2V2.WindowsService.Threads.RequestLogger
{
    public class RequestLoggerDataService
    {
        private static readonly string PageViewInsert = new StringBuilder()
            .Append("insert into PageView (institutionId, userId, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC ")
            .Append(
                "     , ipAddressOctetD, ipAddressInteger, pageViewTimestamp, pageViewRunTime, sessionId, url, requestId ")
            .Append("     , referrer, countryCode, serverNumber, authenticationType, httpMethod) ")
            .Append("values (@InstitutionId, @UserId, @IpAddressOctetA, @IpAddressOctetB, @IpAddressOctetC ")
            .Append(
                "      , @IpAddressOctetD, @IpAddressInteger, @PageViewTimestamp, @PageViewRunTime, @SessionId, @Url, @RequestId ")
            .Append("      , @Referrer, @CountryCode, @ServerNumber, @AuthenticationType, @HttpMethod); ")
            .ToString();

        private static readonly string SearchInsert = new StringBuilder()
            .Append("insert into Search (institutionId, userId, searchTypeId, isArchive, isExternal, ipAddressOctetA ")
            .Append(
                "     , ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, searchTimestamp, requestId) ")
            .Append("values (@InstitutionId, @UserId, @SearchTypeId, @IsArchive, @IsExternal, @IpAddressOctetA ")
            .Append(
                "      , @IpAddressOctetB, @IpAddressOctetC, @IpAddressOctetD, @IpAddressInteger, @SearchTimestamp, @RequestId); ")
            .ToString();

        private static readonly string ContentViewInsert = new StringBuilder()
            .Append(
                "insert into ContentView (institutionId, userId, resourceId, chapterSectionId, turnawayTypeId, ipAddressOctetA, ipAddressOctetB ")
            .Append(
                "     , ipAddressOctetC, ipAddressOctetD, ipAddressInteger, contentViewTimestamp, actionTypeId, foundFromSearch, searchTerm, requestId")
            .Append("     , licenseType, resourceStatusId) ")
            .Append(
                "values (@InstitutionId, @UserId, @ResourceId, @ChapterSectionId, @TurnawayTypeId, @IpAddressOctetA, @IpAddressOctetB ")
            .Append(
                "  , @IpAddressOctetC, @IpAddressOctetD, @IpAddressInteger, @ContentViewTimestamp, @ActionTypeId, @FoundFromSearch, @SearchTerm, @RequestId ")
            .Append("  , @LicenseType, @ResourceStatusId);")
            .ToString();

        private static readonly string MediaViewInsert = new StringBuilder()
            .Append(
                "insert into MediaView (institutionId, userId, resourceId, chapterSectionId, mediaFileName, ipAddressOctetA, ipAddressOctetB ")
            .Append("     , ipAddressOctetC, ipAddressOctetD, ipAddressInteger, mediaViewTimestamp, requestId) ")
            .Append(
                "values (@InstitutionId, @UserId, @ResourceId, @ChapterSectionId, @MediaFileName, @IpAddressOctetA, @IpAddressOctetB ")
            .Append("  , @IpAddressOctetC, @IpAddressOctetD, @IpAddressInteger, @MediaViewTimestamp, @RequestId); ")
            .ToString();

        private readonly ILog<RequestLoggerDataService> _log;
        private readonly R2ReportsDataService _r2ReportsDataService;

        public RequestLoggerDataService(ILog<RequestLoggerDataService> log, R2ReportsDataService r2ReportsDataService)
        {
            _log = log;
            _r2ReportsDataService = r2ReportsDataService;
        }

        public bool SaveRequest(RequestData requestData)
        {
            try
            {
                var expectedRowsEffected = 1;
                var sql = GetSql(requestData);

                var url = requestData.Url.Length < 1000
                    ? requestData.Url
                    : $"{requestData.Url.Substring(0, 1000)} ... TRUNCATED!";

                var parameters = new List<ISqlCommandParameter>
                {
                    new Int32Parameter("InstitutionId", requestData.InstitutionId),
                    new Int32Parameter("UserId", requestData.UserId),
                    new Int16Parameter("IpAddressOctetA", requestData.IpAddress.OctetA),
                    new Int16Parameter("IpAddressOctetB", requestData.IpAddress.OctetB),
                    new Int16Parameter("IpAddressOctetC", requestData.IpAddress.OctetC),
                    new Int16Parameter("IpAddressOctetD", requestData.IpAddress.OctetD),
                    new Int64Parameter("IpAddressInteger", requestData.IpAddress.IntegerValue),
                    new DateTimeParameter("PageViewTimestamp", requestData.RequestTimestamp),
                    new Int32Parameter("PageViewRunTime", requestData.RequestDuration),
                    new StringParameter("SessionId",
                        requestData.Session == null ? "session-id-missing" : requestData.Session.SessionId),
                    new StringParameter("Url", url),
                    new StringParameter("HttpMethod", requestData.HttpMethod),
                    new StringParameter("RequestId", requestData.RequestId),
                    new StringParameter("Referrer", TruncateField(requestData.Referrer, 1024)),
                    new StringParameter("CountryCode", TruncateField(requestData.CountryCode, 10)),
                    new Int32Parameter("ServerNumber", requestData.ServerNumber),
                    new StringParameter("AuthenticationType", requestData.AuthenticationType)
                };

                if (requestData.SearchRequest != null)
                {
                    parameters.Add(new Int32Parameter("SearchTypeId", requestData.SearchRequest.SearchTypeId));
                    parameters.Add(new BooleanParameter("IsArchive", requestData.SearchRequest.IsArchivedSearch));
                    parameters.Add(new BooleanParameter("IsExternal", requestData.SearchRequest.IsExternalSearch));
                    parameters.Add(new DateTimeParameter("SearchTimestamp", requestData.RequestTimestamp));
                    expectedRowsEffected++;
                }

                if (requestData.ContentView != null)
                {
                    parameters.Add(new Int32Parameter("ResourceId", requestData.ContentView.ResourceId));
                    parameters.Add(new StringParameter("ChapterSectionId",
                        TruncateField(requestData.ContentView.ChapterSectionId, 50)));
                    parameters.Add(new Int32Parameter("TurnawayTypeId", requestData.ContentView.ContentTurnawayTypeId));
                    parameters.Add(new DateTimeParameter("ContentViewTimestamp", requestData.RequestTimestamp));
                    parameters.Add(new Int32Parameter("ActionTypeId", requestData.ContentView.ContentActionTypeId));

                    parameters.Add(new BooleanParameter("FoundFromSearch", requestData.ContentView.FoundFromSearch));
                    parameters.Add(new StringParameter("SearchTerm",
                        TruncateField(requestData.ContentView.SearchTerm, 500)));

                    parameters.Add(new Int32Parameter("LicenseType", requestData.ContentView.LicenseTypeId));
                    parameters.Add(new Int32Parameter("ResourceStatusId", requestData.ContentView.ResourceStatusId));

                    expectedRowsEffected++;
                }

                if (requestData.MediaView != null)
                {
                    if (requestData.ContentView == null)
                    {
                        parameters.Add(new Int32Parameter("ResourceId", requestData.MediaView.ResourceId));
                        parameters.Add(new StringParameter("ChapterSectionId",
                            TruncateField(requestData.MediaView.ChapterSectionId, 50)));
                    }

                    parameters.Add(new DateTimeParameter("MediaViewTimestamp", requestData.RequestTimestamp));
                    parameters.Add(new StringParameter("MediaFileName",
                        TruncateField(requestData.MediaView.MediaFileName, 255)));

                    expectedRowsEffected++;
                }

                var rowCount =
                    _r2ReportsDataService.ExecuteInsertStatementReturnRowCount(sql, parameters.ToArray(), true);

                _log.DebugFormat("rowCount: {0}, expectedRowsEffected: {1}", rowCount, expectedRowsEffected);

                if (expectedRowsEffected == rowCount)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder();
                msg.AppendLine(ex.Message);
                msg.AppendFormat("{0}", requestData == null ? "requestDate is null" : requestData.ToDebugString());
                _log.Error(msg.ToString(), ex);
            }

            requestData.FailedSaveAttempts++;
            return false;
        }

        public static string TruncateField(string value, int maxLength)
        {
            return value == null
                ? null
                : value.Length < maxLength
                    ? value
                    : value.Substring(0, maxLength);
        }

        public string GetSql(RequestData requestData)
        {
            var sql = new StringBuilder()
                .AppendLine(PageViewInsert);

            if (requestData.SearchRequest != null)
            {
                sql.Append("\t").AppendLine(SearchInsert);
            }

            if (requestData.ContentView != null)
            {
                sql.Append("\t").AppendLine(ContentViewInsert);
            }

            if (requestData.MediaView != null)
            {
                sql.Append("\t").AppendLine(MediaViewInsert);
            }

            return sql.ToString();
        }
    }
}