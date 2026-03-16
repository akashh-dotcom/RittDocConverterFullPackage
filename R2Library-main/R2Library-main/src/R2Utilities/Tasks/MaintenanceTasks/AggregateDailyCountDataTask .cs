#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.MaintenanceTasks
{
    public class AggregateDailyCountDataTask : TaskBase, ITask
    {
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly ReportDataService _reportDataService;
        private readonly ReportWebDataService _reportWebDataService;
        private string _databaseTable;
        private string _databaseTableTemp;
        private string _delimiter;

        private string _file;

        /// <summary>
        ///     Task written to aggregate the R2reports tables (PageView, ContentView & Search) into the daily tables
        ///     (DailyContentTurnawayCount, DailyContentViewCount, DailyPageViewCount, DailySearchCount, DailySessionCount)
        /// </summary>
        public AggregateDailyCountDataTask(
            ReportDataService reportDataService
            , ReportWebDataService reportWebDataService
            , IR2UtilitiesSettings r2UtilitiesSettings
        )
            : base("AggregateDailyCountData", "-AggregateDailyCountData", "11", TaskGroup.ContentLoading,
                "Task to aggregate daily counts for reports", true)
        {
            _reportDataService = reportDataService;
            _reportWebDataService = reportWebDataService;
            _r2UtilitiesSettings = r2UtilitiesSettings;
        }

        public new void Init(string[] commandLineArguments)
        {
            base.Init(commandLineArguments);

            _file = GetArgument("file");
            _delimiter = GetArgument("delimiter");
            _databaseTable = GetArgument("databasetable");
            _databaseTableTemp = $"{_databaseTable}_Temp";


            Log.Info(
                $"-job: AggregateDailyCountData, -file: {_file}, -delimiter: {_delimiter}, -databasetable: {_databaseTable}");
        }

        public override void Run()
        {
            TaskResult.Information = new StringBuilder()
                .Append("This task will update the daily counts for multiple tables in the R2Reports Database. ")
                .Append("After that it will BCP the data out, zip it up, and re-build the indexes. ")
                .ToString();

            var step = new TaskResultStep { Name = "AggregateDailyCountData", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                if (string.IsNullOrWhiteSpace(_file))
                {
                    var stepResults = new StringBuilder();

                    SetAggregateAndDeleteDates(out var aggregateStart, out var deleteStartDate);

                    var aggregateSuccess = AggregateData(aggregateStart, stepResults);

                    var deleteSuccess = BcpOutAndDeleteData(deleteStartDate, stepResults);

                    _reportDataService.RebuildIndexTables();

                    step.Results = stepResults.ToString();
                    step.CompletedSuccessfully = aggregateSuccess && deleteSuccess;
                }
                else
                {
                    var dataTable = GetBulkInsertDataTable();
                    var rowsErrors = BuildDataTableAndValidateFile(dataTable);
                    var rowsInsert = InsertDataTable(dataTable);

                    CopyFromTemptoDestination();

                    step.Results = $"Rows: {rowsInsert} Insert into {_databaseTable}";
                    step.CompletedSuccessfully = rowsErrors == 0;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                step.CompletedSuccessfully = false;
                step.Results = ex.Message;
                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                UpdateTaskResult();
            }
        }

        #region "Export"

        private void SetAggregateAndDeleteDates(out DateTime aggregateStartDate, out DateTime deleteStartDate)
        {
            //Gets newest Daily Page View and then rounds to the next day month.
            //If the day is not 1 round to the next month. This rounding to the next day will only happen if there was not a single page view on the last day of the month
            //Prevents rerunning the latest month if there is no data for the last day.
            var newestDailyPageView = _reportDataService.GetNewestDailyPageView().AddDays(1);

            if (newestDailyPageView.Day != 1)
            {
                newestDailyPageView = newestDailyPageView.AddMonths(1);
            }

            aggregateStartDate = new DateTime(newestDailyPageView.Year, newestDailyPageView.Month, 1);

            //Gets the oldest Content View so I know when to start deleteing.
            var oldestContentView = _reportDataService.GetOldestContentView();
            deleteStartDate = new DateTime(oldestContentView.Year, oldestContentView.Month, 1);
        }

        private bool AggregateData(DateTime aggregateMonth, StringBuilder results)
        {
            try
            {
                var stopDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                while (aggregateMonth != stopDate)
                {
                    //Insert Data into Daily Count Tables
                    Log.InfoFormat("- - - - - - - - - -");
                    var contentViewCount = _reportDataService.AggregateContentViewCount(aggregateMonth);
                    Log.InfoFormat("- - - - - - - - - -");
                    var contentViewTurnawayCount = _reportDataService.AggregateContentViewTurnawayCount(aggregateMonth);
                    Log.InfoFormat("- - - - - - - - - -");
                    var pageViewCount = _reportDataService.AggregatePageViewCount(aggregateMonth);
                    Log.InfoFormat("- - - - - - - - - -");
                    var pageViewSessionCount = _reportDataService.AggregatePageViewSessionCount(aggregateMonth);
                    Log.InfoFormat("- - - - - - - - - -");
                    var pageViewResourceSessionCount =
                        _reportDataService.AggregatePageViewResourceSessionCount(aggregateMonth);
                    Log.InfoFormat("- - - - - - - - - -");
                    var searchCount = _reportDataService.AggregateSearchCount(aggregateMonth);
                    Log.InfoFormat("- - - - - - - - - -");

                    results.AppendLine()
                        .AppendFormat("Aggregated Daily counts for {0}", aggregateMonth).AppendLine()
                        .AppendFormat("{0} Content View", contentViewCount).AppendLine()
                        .AppendFormat("{0} Content View Turnaway", contentViewTurnawayCount).AppendLine()
                        .AppendFormat("{0} Page View", pageViewCount).AppendLine()
                        .AppendFormat("{0} Session", pageViewSessionCount).AppendLine()
                        .AppendFormat("{0} ResourceSession", pageViewResourceSessionCount).AppendLine()
                        .AppendFormat("{0} Search", searchCount).AppendLine()
                        ;

                    var viewMonth = aggregateMonth.AddMonths(1);

                    _reportWebDataService.AlterDailyContentTurnawayCount(viewMonth);
                    _reportWebDataService.AlterDailyContentViewCount(viewMonth);
                    _reportWebDataService.AlterDailyPageViewCount(viewMonth);
                    _reportWebDataService.AlterDailySearchCount(viewMonth);
                    _reportWebDataService.AlterDailySessionCount(viewMonth);
                    _reportWebDataService.AlterDailyResourceSessionCount(viewMonth);


                    aggregateMonth = aggregateMonth.AddMonths(1);
                }

                if (aggregateMonth != stopDate)
                {
                    //Update Views
                }

                return true;
            }
            catch (Exception ex)
            {
                results.Append(ex.Message);
                return false;
            }
        }

        private bool BcpOutAndDeleteData(DateTime deleteStartDate, StringBuilder results)
        {
            try
            {
                var monthToStop = GetBcpDeleteStopDate();

                if (deleteStartDate < monthToStop)
                {
                    while (deleteStartDate != monthToStop)
                    {
                        var exportSuccess = _reportDataService.BulkExportR2ReportsTables(deleteStartDate);

                        var contentViewRowsDeleted = 0;
                        var pageViewRowsDeleted = 0;
                        var searchRowsDeleted = 0;

                        if (exportSuccess)
                        {
                            contentViewRowsDeleted = _reportDataService.DeleteRangeFromTable(deleteStartDate,
                                "ContentView", "cast(contentViewTimestamp as date)");
                            pageViewRowsDeleted = _reportDataService.DeleteRangeFromTable(deleteStartDate, "PageView",
                                "cast(pageViewTimestamp as date)");
                            searchRowsDeleted = _reportDataService.DeleteRangeFromTable(deleteStartDate, "Search",
                                "cast(searchTimestamp as date)");
                        }

                        results.AppendLine()
                            .AppendFormat("Removed data for {0}", deleteStartDate).AppendLine()
                            .AppendFormat("{0} PageView rows deleted.", contentViewRowsDeleted).AppendLine()
                            .AppendFormat("{0} ContentView rows deleted.", pageViewRowsDeleted).AppendLine()
                            .AppendFormat("{0} Search rows deleted.", searchRowsDeleted).AppendLine();

                        deleteStartDate = deleteStartDate.AddMonths(1);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                results.Append(ex.Message);
                return false;
            }
        }

        private DateTime GetBcpDeleteStopDate()
        {
            var baseMonthToStop = DateTime.Now.AddMonths(-_r2UtilitiesSettings.AggregateDailyCountMonthsToGoBack);
            var monthToStop =
                new DateTime(baseMonthToStop.Year, baseMonthToStop.Month, 1); //Date to stop deleting/BCP out data.
            return monthToStop;
        }

        #endregion

        #region "Import"

        private DataTable GetBulkInsertDataTable()
        {
            try
            {
                var dataTable = new DataTable();

                using (var connection = new SqlConnection(_r2UtilitiesSettings.R2ReportsConnection))
                {
                    connection.Open();


                    var sb = new StringBuilder()
                        .Append("select ")
                        .Append("CASE WHEN DATA_TYPE = 'varchar' then 'System.String' ")
                        .Append("WHEN DATA_TYPE = 'nvarchar' then 'System.String' ")
                        .Append("WHEN DATA_TYPE = 'datetime' then 'System.DateTime' ")
                        .Append("WHEN DATA_TYPE = 'smalldatetime' then 'System.DateTime' ")
                        .Append("WHEN DATA_TYPE = 'bit' then 'System.Int32' ")
                        .Append("WHEN DATA_TYPE = 'int' then 'System.Int32' ")
                        .Append("when DATA_TYPE = 'tinyint' then 'System.Int32' ")
                        .Append("when DATA_TYPE = 'char' then 'System.String' ")
                        .Append("when DATA_TYPE = 'money' then 'System.Decimal' ")
                        .Append("when DATA_TYPE = 'float' then 'System.Decimal' ")
                        .Append("when DATA_TYPE = 'decimal' then 'System.Decimal' ")
                        .Append("when DATA_TYPE = 'smallint' then 'System.Int16' ")
                        .Append("when DATA_TYPE = 'bigint' then 'System.Int64' ")
                        .Append("when DATA_TYPE = 'varbinary' then 'System.Byte[]' ")
                        .Append("when DATA_TYPE = 'text' then 'System.String' ")
                        .Append("END, COLUMN_NAME, IS_NULLABLE from ")
                        .Append(
                            $"INFORMATION_SCHEMA.COLUMNS IC where TABLE_NAME = '{_databaseTableTemp.Replace("dbo.", "")}' ")
                        .ToString();

                    var tableInformationCommand = new SqlCommand(sb, connection);

                    var reader = tableInformationCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        var datatype = reader.GetString(0);
                        var columnName = reader.GetString(1);
                        var allowNull = reader.GetString(2);
                        dataTable.Columns.Add(new DataColumn
                        {
                            ColumnName = columnName,
                            DataType = Type.GetType(datatype),
                            AllowDBNull = allowNull.ToLower() == "yes",
                            DefaultValue = null
                        });
                    }

                    reader.Close();
                }

                Log.Debug("GetBulkInsertDataTable was successfull");
                return dataTable;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat(ex.Message, ex);
                throw;
            }
        }

        private int BuildDataTableAndValidateFile(DataTable dataTable)
        {
            var rowsErrors = 0;
            using (var parser = new TextFieldParser(_file))
            {
                parser.HasFieldsEnclosedInQuotes = false;
                if (_delimiter == "\\t")
                {
                    parser.SetDelimiters("\t");
                }
                else
                {
                    parser.SetDelimiters(_delimiter);
                }

                var columnCount = dataTable.Columns.Count;
                var rowCount = 0;

                while (!parser.EndOfData)
                {
                    var skipRow = false;
                    rowCount++;
                    var fields = parser.ReadFields();
                    var row = dataTable.NewRow();
                    var i = 0;

                    try
                    {
                        if (fields != null)
                        {
                            if (fields.Length > columnCount)
                            {
                                WriteErrorToFile(fields);
                                rowsErrors++;
                                skipRow = true;
                            }
                            else
                            {
                                foreach (var field in fields)
                                {
                                    if (field == null)
                                    {
                                        row.SetField(i, DBNull.Value);
                                    }
                                    else
                                    {
                                        row.SetField(i, field);
                                    }

                                    i++;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message, ex);
                        throw;
                    }

                    if (!skipRow)
                    {
                        dataTable.Rows.Add(row);
                    }
                }
            }

            return rowsErrors;
        }

        private void WriteErrorToFile(string[] errorRow)
        {
            var errorFile = _file.Replace(".dat", "_Error.dat");
            if (!File.Exists(errorFile))
            {
                using (var sw = File.CreateText(errorFile))
                {
                    sw.WriteLine(string.Join("\t", errorRow));
                }
            }
            else
            {
                using (var sw = File.AppendText(errorFile))
                {
                    sw.WriteLine(string.Join("\t", errorRow));
                }
            }
        }

        private int InsertDataTable(DataTable dataTable)
        {
            int rows;
            try
            {
                var tSql = $"Select Count(*) from {_databaseTableTemp};";

                var rowCount = ExecuteBasicCountQuery(tSql, new List<ISqlCommandParameter>(), true,
                    _r2UtilitiesSettings.R2ReportsConnection);
                Log.Info($"Rows Before Insert: {rowCount}");

                tSql = $"truncate table {_databaseTableTemp};";
                rowCount = ExecuteBasicCountQuery(tSql, new List<ISqlCommandParameter>(), true,
                    _r2UtilitiesSettings.R2ReportsConnection);
                Log.Debug($"Rows Truncated: {rowCount}");

                using (var connection = new SqlConnection(_r2UtilitiesSettings.R2ReportsConnection))
                {
                    connection.Open();

                    using (var bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.DestinationTableName = _databaseTableTemp;

                        bulkCopy.WriteToServer(dataTable);
                    }
                }

                var sql = $"Select Count(*) from {_databaseTableTemp};";

                rows = ExecuteBasicCountQuery(sql, new List<ISqlCommandParameter>(), true,
                    _r2UtilitiesSettings.R2ReportsConnection);

                Log.Debug($"Rows Inserted: {rows}");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }

            return rows;
        }

        private void CopyFromTemptoDestination()
        {
            var sqlBuilder = new StringBuilder();
            if (_databaseTable.IndexOf("pageview", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sqlBuilder.Append(
                    $"INSERT INTO {_databaseTable}(institutionId, userId, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger ");
                sqlBuilder.Append(
                    ", pageViewTimestamp, pageViewRunTime, sessionId, url, requestId, referrer, countryCode, serverNumber, authenticationType) ");
                sqlBuilder.Append(
                    "select institutionId, userId, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger ");
                sqlBuilder.Append(
                    ", pageViewTimestamp, pageViewRunTime, sessionId, url, requestId, referrer, countryCode, serverNumber, authenticationType ");
                sqlBuilder.Append($"from {_databaseTableTemp}");
            }

            if (_databaseTable.IndexOf("search", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sqlBuilder.Append(
                    $"INSERT INTO {_databaseTable}(institutionId , userId , searchTypeId , isArchive , isExternal , ipAddressOctetA ");
                sqlBuilder.Append(
                    ",ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, requestId, searchTimestamp) ");
                sqlBuilder.Append(
                    "select institutionId, userId, searchTypeId, isArchive, isExternal, ipAddressOctetA ");
                sqlBuilder.Append(
                    ", ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, requestId, searchTimestamp ");
                sqlBuilder.Append($"from {_databaseTableTemp}");
            }

            if (_databaseTable.IndexOf("contentview", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sqlBuilder.Append(
                    $"INSERT INTO {_databaseTable}(institutionId, userId, resourceId, chapterSectionId, turnawayTypeId, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC ");
                sqlBuilder.Append(
                    ", ipAddressOctetD, ipAddressInteger, contentViewTimestamp, actionTypeId, foundFromSearch, searchTerm, requestId, licenseType, resourceStatusId) ");
                sqlBuilder.Append(
                    "select institutionId, userId, resourceId, chapterSectionId, turnawayTypeId, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD ");
                sqlBuilder.Append(
                    ", ipAddressInteger, contentViewTimestamp, actionTypeId, foundFromSearch, searchTerm, requestId, isnull(licenseType, 0), resourceStatusId ");
                sqlBuilder.Append($"from {_databaseTableTemp}");
            }

            SqlConnection cnn = null;
            SqlCommand command = null;

            try
            {
                cnn = GetConnection(_r2UtilitiesSettings.R2ReportsConnection);

                //This is set to be exactly like SSMS. ADO.NET turns this off by default. This results in different execution plan thatn SSMS.
                command = new SqlCommand("SET ARITHABORT ON", cnn);
                command.ExecuteNonQuery();


                command = GetSqlCommand(cnn, sqlBuilder.ToString(), null, 300, null);

                var rows = command.ExecuteNonQuery();
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

            //ExecuteStatement(sqlBuilder.ToString(), true, _r2UtilitiesSettings.R2ReportsConnection);
        }

        #endregion
    }
}