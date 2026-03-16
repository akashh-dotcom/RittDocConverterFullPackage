#region

using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using log4net;

#endregion

namespace R2Library.Data.ADO.Core
{
    public class DbConnectionHelper
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public string ConnectionString { get; set; }


        protected SqlConnection GetConnection()
        {
            return GetConnection(ConnectionString);
        }

        protected SqlConnection GetConnection(string connectionString)
        {
            try
            {
                var cnn = new SqlConnection(connectionString);
                if (cnn.State != ConnectionState.Open)
                {
                    cnn.Open();
                }

                return cnn;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected void DisposeConnections(SqlConnection cnn, SqlCommand command, SqlDataReader reader)
        {
            if (null != reader)
            {
                reader.Dispose();
            }

            DisposeConnections(cnn, command);
        }

        protected void DisposeConnections(SqlConnection cnn, SqlCommand command)
        {
            if (null != command)
            {
                command.Dispose();
            }

            DisposeConnections(cnn);
        }

        protected void DisposeConnections(SqlConnection cnn)
        {
            if (null != cnn)
            {
                cnn.Dispose();
            }
        }

        /// <summary>
        /// </summary>
        protected static string GetDatabaseName(string connectionString)
        {
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            Log.DebugFormat("database: {0}", sqlConnectionStringBuilder["database"]);
            return sqlConnectionStringBuilder["database"].ToString();
        }
    }
}