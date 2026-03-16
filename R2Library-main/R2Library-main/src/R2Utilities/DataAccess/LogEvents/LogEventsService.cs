#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2Library.Data.ADO.R2.DataServices;
using R2Utilities.Infrastructure.Settings;

#endregion

namespace R2Utilities.DataAccess.LogEvents
{
    public class LogEventsService : DataServiceBase
    {
        public LogEventsService(IR2UtilitiesSettings r2UtilitiesSettings) : base(
            r2UtilitiesSettings.LogEventsConnection)
        {
        }

        public List<LogEvent> GetLogEvents(ReportConfiguration reportConfiguration, string tableName, DateTime start,
            DateTime end)
        {
            var sqlBuilder = new StringBuilder()
                    .Append($" select {reportConfiguration.FieldSelect} from {tableName} ")
                    .Append(" where timestamp >= @StartDate and timestamp <= @EndDate ")
                ;

            if (reportConfiguration.LevelInt > 0)
            {
                sqlBuilder.Append($" and levelInt = {reportConfiguration.LevelInt} ");
            }

            if (!string.IsNullOrWhiteSpace(reportConfiguration.WhereClause))
            {
                sqlBuilder.Append($" and {reportConfiguration.WhereClause} ");
            }

            if (!string.IsNullOrWhiteSpace(reportConfiguration.Grouping))
            {
                sqlBuilder.Append($" group by {reportConfiguration.Grouping} ");
            }

            if (!string.IsNullOrWhiteSpace(reportConfiguration.Having))
            {
                sqlBuilder.Append($" having {reportConfiguration.Having} ");
            }

            if (!string.IsNullOrWhiteSpace(reportConfiguration.OrderByColumnNumber))
            {
                sqlBuilder.Append($" order by {reportConfiguration.OrderByColumnNumber} ");
            }


            var parameters = new List<ISqlCommandParameter>
            {
                new DateTimeParameter("StartDate", start),
                new DateTimeParameter("EndDate", end)
            };


            return GetEntityList<LogEvent>(sqlBuilder.ToString(), parameters, true);
        }

        public List<LogEventsConfiguration> GetLogEventsConfigruation(string fileNameAndPath)
        {
            var logEventsConfigurations = new List<LogEventsConfiguration>();
            if (!string.IsNullOrWhiteSpace(fileNameAndPath))
            {
                var file = new FileInfo(fileNameAndPath);
                if (file.Exists)
                {
                    var jsonFileText = File.ReadAllText(fileNameAndPath);
                    var logEventsConfiguration = JsonConvert.DeserializeObject<LogEventsConfiguration>(jsonFileText);
                    logEventsConfigurations.Add(logEventsConfiguration);
                }
            }

            return logEventsConfigurations;
        }
    }
}