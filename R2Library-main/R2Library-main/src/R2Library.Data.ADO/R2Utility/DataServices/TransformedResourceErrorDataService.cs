#region

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;

#endregion

namespace R2Library.Data.ADO.R2Utility.DataServices
{
    public class TransformedResourceErrorDataService : DataServiceBase
    {
        private const bool LogSql = true;

        private static readonly string _insert = new StringBuilder()
            .Append("insert into TransformedResourceError(resourceId, isbn, filename, dateCompleted, errorMessage) ")
            .Append("values (@ResourceId, @Isbn, @Filename, @DateCompleted, @ErrorMessage);")
            .ToString();

        public int Insert(TransformedResourceError error)
        {
            var stopwatch = new Stopwatch();

            SqlConnection cnn = null;
            SqlCommand command = null;

            try
            {
                var parameters = new List<ISqlCommandParameter>
                {
                    new Int32Parameter("ResourceId", error.ResourceId),
                    new StringParameter("Isbn", error.Isbn),
                    new DateTimeParameter("DateCompleted", error.DateCreated),
                    new StringParameter("Filename", error.Filename),
                    new StringParameter("ErrorMessage", error.ErrorMessage)
                };


                cnn = GetConnection();
                command = GetSqlCommand(cnn, _insert, parameters);

                if (LogSql)
                {
                    LogCommandDebug(command);
                    stopwatch.Start();
                }

                var rows = command.ExecuteNonQuery();

                if (LogSql)
                {
                    stopwatch.Stop();
                    Log.DebugFormat("rows effected: {0}, insert time: {1}ms", rows, stopwatch.ElapsedMilliseconds);
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
    }
}