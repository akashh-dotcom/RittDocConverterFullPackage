#region

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using log4net;
using R2Library.Data.ADO.Core.SqlCommandParameters;

#endregion

namespace R2Library.Data.ADO.Core
{
    public class DbCommandHelper : DbConnectionHelper
    {
        protected new static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected SqlCommand GetSqlCommand(SqlConnection cnn, string commandText)
        {
            return GetSqlCommand(cnn, commandText, null, 30, null);
        }

        protected SqlCommand GetSqlCommand(SqlConnection cnn, string commandText,
            IEnumerable<ISqlCommandParameter> sqlCommandParameters)
        {
            return GetSqlCommand(cnn, commandText, sqlCommandParameters, 30, null);
        }

        protected SqlCommand GetSqlCommand(SqlConnection cnn, string commandText,
            IEnumerable<ISqlCommandParameter> sqlCommandParameters, int commandTimeout, SqlTransaction transaction)
        {
            if (cnn == null)
            {
                Log.Warn("connection object is null");
                return null;
            }

            var command = cnn.CreateCommand();
            command.CommandText = commandText;
            command.CommandTimeout = commandTimeout;

            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            if (sqlCommandParameters != null)
            {
                foreach (var parameter in sqlCommandParameters)
                {
                    parameter.SetCommandParmater(command);
                }
            }

            return command;
        }

        public static void SetCommandParmater(SqlCommand command, string parameterName, string parameterValue)
        {
            if (null == parameterValue)
            {
                command.Parameters.AddWithValue($"@{parameterName}", DBNull.Value);
            }
            else
            {
                command.Parameters.AddWithValue($"@{parameterName}", parameterValue);
            }
        }

        public static void SetCommandParmater(SqlCommand command, string parameterName, string parameterValue,
            bool useEmptyString)
        {
            if (null == parameterValue)
            {
                if (useEmptyString)
                {
                    command.Parameters.AddWithValue($"@{parameterName}", string.Empty);
                }
                else
                {
                    command.Parameters.AddWithValue($"@{parameterName}", DBNull.Value);
                }
            }
            else
            {
                command.Parameters.AddWithValue($"@{parameterName}", parameterValue);
            }
        }

        public static void SetCommandParmater(SqlCommand command, string parameterName, bool parameterValue)
        {
            command.Parameters.AddWithValue($"@{parameterName}", parameterValue);
        }

        protected static void SetCommandParmater(SqlCommand command, string parameterName, int parameterValue)
        {
            command.Parameters.AddWithValue($"@{parameterName}", parameterValue);
        }

        public static void SetCommandParmater(SqlCommand command, string parameterName, decimal parameterValue)
        {
            command.Parameters.AddWithValue($"@{parameterName}", parameterValue);
        }

        public static void SetCommandParmater(SqlCommand command, string parameterName, decimal? parameterValue)
        {
            if (parameterValue != null)
            {
                command.Parameters.AddWithValue($"@{parameterName}", parameterValue);
            }
            else
            {
                command.Parameters.AddWithValue($"@{parameterName}", DBNull.Value);
            }
        }

        public static void SetCommandParmater(SqlCommand command, string parameterName, DateTime parameterValue)
        {
            if (parameterValue > DateTime.MinValue)
            {
                command.Parameters.AddWithValue($"@{parameterName}", parameterValue);
            }
            else
            {
                command.Parameters.AddWithValue($"@{parameterName}", DBNull.Value);
            }
        }

        public static void SetCommandParmater(SqlCommand command, string parameterName, DateTime? parameterValue)
        {
            if (parameterValue != null)
            {
                command.Parameters.AddWithValue($"@{parameterName}", parameterValue);
            }
            else
            {
                command.Parameters.AddWithValue($"@{parameterName}", DBNull.Value);
            }
        }

        public static void SetCommandParmater(SqlCommand command, string parameterName, byte[] parameterValue)
        {
            if (null == parameterValue)
            {
                command.Parameters.AddWithValue($"@{parameterName}", DBNull.Value);
            }
            else
            {
                command.Parameters.AddWithValue($"@{parameterName}", parameterValue);
            }
        }

        protected void LogCommandInfo(SqlCommand command)
        {
            if (command == null)
            {
                Log.Warn("command object is null");
                return;
            }

            try
            {
                Log.InfoFormat("CommandTimeout: {0}, CommandType: {1}", command.CommandTimeout, command.CommandType);
                Log.InfoFormat("CommandText: {0}", command.CommandText);

                var msg = new StringBuilder();
                foreach (SqlParameter parameter in command.Parameters)
                {
                    msg.AppendFormat("{0} = {1}, ", parameter.ParameterName, parameter.Value);
                }

                if (msg.Length <= 0)
                {
                    return;
                }

                msg.Replace(", ", "]", msg.Length - 3, 1);
                msg.Insert(0, "Parameters: [");
                Log.Debug(msg.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        protected void LogCommandDebug(SqlCommand command)
        {
            if (command == null)
            {
                Log.Info("command object is null");
                return;
            }

            try
            {
                Log.DebugFormat("CommandText: {0}", command.CommandText);
                var sb = new StringBuilder();
                foreach (SqlParameter parameter in command.Parameters)
                {
                    sb.AppendFormat("{0}{1} = {2}", sb.Length > 0 ? ", " : string.Empty, parameter.ParameterName,
                        parameter.Value);
                }

                if (sb.Length > 0)
                {
                    Log.Debug(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }
    }
}