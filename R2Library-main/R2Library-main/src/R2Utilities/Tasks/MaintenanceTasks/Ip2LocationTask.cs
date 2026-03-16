#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.Infrastructure.Settings;
using R2V2.Infrastructure.Compression;

#endregion

namespace R2Utilities.Tasks.MaintenanceTasks
{
    public class Ip2LocationTask : TaskBase
    {
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;

        public Ip2LocationTask(IR2UtilitiesSettings r2UtilitiesSettings)
            : base(
                "Ip2LocationTask", "-Ip2LocationTask", "25", TaskGroup.DiagnosticsMaintenance,
                "Task will load Ip2Location data", true)
        {
            _r2UtilitiesSettings = r2UtilitiesSettings;
        }

        public override void Run()
        {
            TaskResult.Information = "This task will download the latest Ip2Location database and update the database.";
            var step = new TaskResultStep { Name = "Ip2LocationTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                DownloadFileAndExtract(_r2UtilitiesSettings.Ip2LocationWorkingFolder);

                var ip2LocationFile = Path.Combine(_r2UtilitiesSettings.Ip2LocationWorkingFolder, "IPCountry.csv");
                CopyFileToLocations(ip2LocationFile);

                var rowsInserted = ProcessFile(ip2LocationFile);

                step.Results = $"Ip2Location Rows Insertred: {rowsInserted}";
                step.CompletedSuccessfully = rowsInserted > 0;

                Directory.Delete(_r2UtilitiesSettings.Ip2LocationWorkingFolder, true);
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

        private int ProcessFile(string ip2LocationFile)
        {
            try
            {
                var baseDataTable = GetBulkInsertDataTable(_r2UtilitiesSettings.Ip2LocationTableName);
                var populatedDataTable = BuildDataTableAndValidateFile(baseDataTable, ip2LocationFile, ",");

                InsertIp2LocationData(populatedDataTable);

                return UpdateWebDatabase();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        private void DownloadFileAndExtract(string workingDirectory)
        {
            try
            {
                var filePathAndName = GetDownloadFileName(workingDirectory);

                var webClient = new WebClient();
                webClient.DownloadFile(_r2UtilitiesSettings.Ip2LocationDownloadUrl, filePathAndName);

                ZipHelper.ExtractAll(filePathAndName, workingDirectory);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        private string GetDownloadFileName(string workingDirectory)
        {
            if (Directory.Exists(workingDirectory))
            {
                Directory.Delete(workingDirectory, true);
            }

            Directory.CreateDirectory(workingDirectory);


            var fileName = $"Ip2Location-{DateTime.Now.ToString("yyyy-MM-dd_HHmmss")}.zip";
            var filePathAndName = Path.Combine(workingDirectory, fileName);
            return filePathAndName;
        }

        private DataTable BuildDataTableAndValidateFile(DataTable dataTable, string fileNameAndPath, string delimiter)
        {
            using (var parser = new TextFieldParser(fileNameAndPath))
            {
                parser.HasFieldsEnclosedInQuotes = true;
                parser.SetDelimiters(delimiter);
                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    var row = dataTable.NewRow();
                    var i = 0;
                    try
                    {
                        if (fields != null)
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
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message, ex);
                        throw;
                    }

                    dataTable.Rows.Add(row);
                }
            }

            return dataTable;
        }

        private DataTable GetBulkInsertDataTable(string databaseTableName)
        {
            try
            {
                var dataTable = new DataTable();

                using (var connection = new SqlConnection(_r2UtilitiesSettings.Ip2LocationConnection))
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
                        .Append("when DATA_TYPE = 'char' then 'System.String' ")
                        .Append("when DATA_TYPE = 'money' then 'System.Decimal' ")
                        .Append("when DATA_TYPE = 'float' then 'System.Decimal' ")
                        .Append("when DATA_TYPE = 'decimal' then 'System.Decimal' ")
                        .Append("when DATA_TYPE = 'smallint' then 'System.Int16' ")
                        .Append("when DATA_TYPE = 'bigint' then 'System.Int64' ")
                        .Append("when DATA_TYPE = 'varbinary' then 'System.Byte[]' ")
                        .Append("when DATA_TYPE = 'tinyint' then 'System.Boolean' ")
                        .Append("when DATA_TYPE = 'text' then 'System.String' ")
                        .Append("END, COLUMN_NAME, IS_NULLABLE from ")
                        .AppendFormat("INFORMATION_SCHEMA.COLUMNS IC where TABLE_NAME = '{0}' ",
                            databaseTableName.Replace("dbo.", ""))
                        .ToString();

                    //sb = string.Format("Select * from {0}", databaseTableName);
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


        private void InsertIp2LocationData(DataTable dataTable)
        {
            try
            {
                var tSql = new StringBuilder()
                    .AppendFormat("Select Count(*) from {0};", _r2UtilitiesSettings.Ip2LocationTableName).ToString();

                var rows = ExecuteBasicCountQuery(tSql, new List<ISqlCommandParameter>(), true,
                    _r2UtilitiesSettings.Ip2LocationConnection);

                tSql = new StringBuilder()
                    .AppendFormat("truncate table {0};", _r2UtilitiesSettings.Ip2LocationTableName).ToString();

                ExecuteBasicCountQuery(tSql, new List<ISqlCommandParameter>(), true,
                    _r2UtilitiesSettings.Ip2LocationConnection);

                Log.DebugFormat("Rows Truncated: {0}", rows);

                using (var connection = new SqlConnection(_r2UtilitiesSettings.Ip2LocationConnection))
                {
                    connection.Open();

                    using (var bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.DestinationTableName = _r2UtilitiesSettings.Ip2LocationTableName;

                        bulkCopy.WriteToServer(dataTable);
                    }
                }

                var sql = new StringBuilder()
                    .AppendFormat("Select Count(*) from {0};", _r2UtilitiesSettings.Ip2LocationTableName).ToString();

                rows = ExecuteBasicCountQuery(sql, new List<ISqlCommandParameter>(), true,
                    _r2UtilitiesSettings.Ip2LocationConnection);

                Log.DebugFormat("Rows Inserted: {0}", rows);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        private int UpdateWebDatabase()
        {
            var sql = new StringBuilder()
                .AppendFormat("Select Count(*) from tIp2Location;").ToString();

            var rows = ExecuteBasicCountQuery(sql, new List<ISqlCommandParameter>(), true,
                _r2UtilitiesSettings.R2DatabaseConnection);

            sql = new StringBuilder()
                .AppendFormat("truncate table tIp2Location;").ToString();

            ExecuteBasicCountQuery(sql, new List<ISqlCommandParameter>(), true,
                _r2UtilitiesSettings.R2DatabaseConnection);

            Log.DebugFormat("tIp2Location Rows Truncated: {0}", rows);

            sql = new StringBuilder()
                .Append("insert into tIp2Location (iIpTo,iIpFrom,vchCountryCode,vchCountryName)")
                .Append("           select ip_from, ip_to, country_code, country_name")
                .AppendFormat("     from   {0}..{1}", _r2UtilitiesSettings.Ip2LocationDatabaseName,
                    _r2UtilitiesSettings.Ip2LocationTableName)
                .Append("           order by  ip_from")
                .ToString();

            rows = ExecuteInsertStatementReturnRowCount(sql, null, true, _r2UtilitiesSettings.R2DatabaseConnection);

            Log.DebugFormat("tIp2Location Rows Inserted: {0}", rows);
            return rows;
        }


        private void CopyFileToLocations(string originalFileLocation)
        {
            try
            {
                var fileLocationsString = _r2UtilitiesSettings.Ip2LocationFileDestinations;
                var destinationFileLocations =
                    fileLocationsString.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                if (destinationFileLocations.Any())
                {
                    var fileInfo = new FileInfo(originalFileLocation);

                    foreach (var destinationFileLocation in destinationFileLocations)
                    {
                        var newFile = Path.Combine(destinationFileLocation, fileInfo.Name);
                        File.Copy(fileInfo.FullName, newFile, true);
                    }
                }
            }

            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }
    }
}