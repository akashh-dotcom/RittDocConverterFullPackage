#region

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using log4net;
using R2Library.Data.ADO.Core.SqlCommandParameters;

#endregion

namespace R2Library.Data.ADO.Core
{
    public class FactoryBase : DbReaderHelper
    {
        // http://www.lammertbies.nl/comm/info/ascii-characters.html#spac
        protected const string ControlCharacterStartOfHeader = "\x1"; // 1
        protected const string ControlCharacterStartOfText = "\x2"; // 2
        protected const string ControlCharacterEndOfText = "\x3"; // 3
        protected const string ControlCharacterEndOfTransmission = "\x4"; // 4
        protected const string ControlCharacterEnquiry = "\x5"; // 5
        protected const string ControlCharacterAcknowledgment = "\x6"; // 6
        protected const string ControlCharacterAudibleBell = "\x7"; // 7
        protected const string ControlCharacterBackspace = "\x8"; // 8
        protected const string ControlCharacterVerticalTab = "\xb"; // 11
        protected const string ControlCharacterFormFeed = "\xc"; // 12
        protected const string ControlCharacterShiftOut = "\xe"; // 14
        protected const string ControlCharacterShiftIn = "\xf"; // 15
        protected const string ControlCharacterDataLinkEscape = "\x10"; // 16
        protected const string ControlCharacterXon = "\x11"; // 17
        protected const string ControlCharacterDc2 = "\x12"; // 18
        protected const string ControlCharacterXoff = "\x13"; // 19
        protected const string ControlCharacterDc4 = "\x14"; // 20
        protected const string ControlCharacterNegativeAcknowledgment = "\x15"; // 21
        protected const string ControlCharacterSynchronousIdle = "\x16"; // 22
        protected const string ControlCharacterEndOfTransmissionBlock = "\x17"; // 23
        protected const string ControlCharacterCancel = "\x18"; // 24
        protected const string ControlCharacterEndOfMedium = "\x19"; // 25
        protected const string ControlCharacterSubstituteChar = "\x1a"; // 26
        protected const string ControlCharacterEscape = "\x1b"; // 27
        protected const string ControlCharacterFileSeparator = "\x1c"; // 28
        protected const string ControlCharacterGroupSeparator = "\x1d"; // 29
        protected const string ControlCharacterRecordSeparator = "\x1e"; // 30
        protected const string ControlCharacterUnitSeparator = "\x1f"; // 31
        protected const string ControlCharacterDelete = "\x7f"; // 127
        protected new static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public int ExecuteInsertStatementReturnIdentity(string insertStatement,
            List<ISqlCommandParameter> sqlCommandParameters, bool logSql)
        {
            return ExecuteInsertStatementReturnIdentity(insertStatement, sqlCommandParameters.ToArray(), logSql);
        }

        public int ExecuteInsertStatementReturnIdentity(string insertStatement,
            ISqlCommandParameter[] sqlCommandParameters, bool logSql)
        {
            return ExecuteInsertStatementReturnIdentity(insertStatement, sqlCommandParameters, logSql,
                ConnectionString);
        }

        public int ExecuteInsertStatementReturnIdentity(string insertStatement,
            ISqlCommandParameter[] sqlCommandParameters, bool logSql, string connectionString)
        {
            var id = -1;
            SqlConnection cnn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            var stopwatch = new Stopwatch();

            var sql = $"{insertStatement}; select @@identity;";
            try
            {
                cnn = GetConnection(connectionString);
                command = GetSqlCommand(cnn, sql, sqlCommandParameters);

                if (logSql)
                {
                    LogCommandDebug(command);
                    stopwatch.Start();
                }

                reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var newId = reader.GetDecimal(0);
                    id = decimal.ToInt32(newId);
                }

                if (logSql)
                {
                    stopwatch.Stop();
                    Log.DebugFormat("new id: {0}, insert time: {1}ms", id, stopwatch.ElapsedMilliseconds);
                }

                return id;
            }
            catch (Exception ex)
            {
                LogCommandInfo(command);
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                DisposeConnections(cnn, command, reader);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="transaction"> </param>
        public int ExecuteInsertStatementReturnIdentity(string insertStatement,
            ISqlCommandParameter[] sqlCommandParameters, bool logSql, SqlConnection cnn, SqlTransaction transaction)
        {
            var id = -1;
            SqlCommand command = null;
            SqlDataReader reader = null;

            var stopwatch = new Stopwatch();

            var sql = $"{insertStatement}; select @@identity;";
            try
            {
                command = GetSqlCommand(cnn, sql, sqlCommandParameters, 15, transaction);

                if (logSql)
                {
                    LogCommandDebug(command);
                    stopwatch.Start();
                }

                reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var newId = reader.GetDecimal(0);
                    id = decimal.ToInt32(newId);
                }

                if (logSql)
                {
                    stopwatch.Stop();
                    Log.DebugFormat("new id: {0}, insert time: {1}ms", id, stopwatch.ElapsedMilliseconds);
                }

                return id;
            }
            catch (Exception ex)
            {
                LogCommandInfo(command);
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                DisposeConnections(null, command, reader);
            }
        }


        public int ExecuteInsertStatementReturnRowCount(string insertStatement,
            ISqlCommandParameter[] sqlCommandParameters, bool logSql, string connectionString = null)
        {
            var stopwatch = new Stopwatch();

            SqlConnection cnn = null;
            SqlCommand command = null;

            var sql = $"{insertStatement};";
            try
            {
                cnn = GetConnection(connectionString ?? ConnectionString);
                command = GetSqlCommand(cnn, sql, sqlCommandParameters);

                if (logSql)
                {
                    LogCommandDebug(command);
                    stopwatch.Start();
                }

                var rows = command.ExecuteNonQuery();

                if (logSql)
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

        public int ExecuteUpdateStatement(string updateStatement, List<ISqlCommandParameter> sqlCommandParameters,
            bool logSql)
        {
            return ExecuteUpdateStatement(updateStatement, sqlCommandParameters.ToArray(), logSql);
        }

        public int ExecuteUpdateStatement(string updateStatement, ISqlCommandParameter[] sqlCommandParameters,
            bool logSql)
        {
            return ExecuteUpdateStatement(updateStatement, sqlCommandParameters, logSql, ConnectionString);
        }

        public int ExecuteUpdateStatement(string updateStatement, ISqlCommandParameter[] sqlCommandParameters,
            bool logSql, string connectionString)
        {
            SqlConnection cnn = null;
            SqlCommand command = null;

            var stopwatch = new Stopwatch();

            try
            {
                cnn = GetConnection(connectionString);
                command = GetSqlCommand(cnn, updateStatement, sqlCommandParameters);

                if (logSql)
                {
                    LogCommandDebug(command);
                    stopwatch.Start();
                }

                var rows = command.ExecuteNonQuery();

                if (logSql)
                {
                    stopwatch.Stop();
                    Log.DebugFormat("rows effected: {0}, update time: {1}ms", rows, stopwatch.ElapsedMilliseconds);
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

        public int ExecuteUpdateStatement(string updateStatement, ISqlCommandParameter[] sqlCommandParameters,
            bool logSql, SqlConnection cnn, SqlTransaction transaction)
        {
            SqlCommand command = null;
            var stopwatch = new Stopwatch();

            try
            {
                command = GetSqlCommand(cnn, updateStatement, sqlCommandParameters, 15, transaction);

                if (logSql)
                {
                    LogCommandDebug(command);
                    stopwatch.Start();
                }

                var rows = command.ExecuteNonQuery();

                if (logSql)
                {
                    stopwatch.Stop();
                    Log.DebugFormat("rows effected: {0}, update time: {1}ms", rows, stopwatch.ElapsedMilliseconds);
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
                DisposeConnections(null, command);
            }
        }

        public int ExecuteTrancateTable(string tableName, string connectionString)
        {
            SqlConnection cnn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            var stopwatch = new Stopwatch();

            var sql = string.Format("select count(*) from {0}; truncate table {0};", tableName);

            try
            {
                cnn = GetConnection(connectionString);
                command = GetSqlCommand(cnn, sql);

                LogCommandDebug(command);
                stopwatch.Start();

                reader = command.ExecuteReader();

                var rows = -3;
                if (reader.Read())
                {
                    rows = GetInt32Value(reader, 0, -5);
                }

                //int rows = command.ExecuteNonQuery();

                stopwatch.Stop();
                Log.DebugFormat("rows effected: {0}, update time: {1}ms", rows, stopwatch.ElapsedMilliseconds);
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
                DisposeConnections(cnn, command, reader);
            }
        }

        public int ExecuteTwoStatementsReturnSecondResult(string statement, ISqlCommandParameter[] sqlCommandParameters,
            bool logSql, string connectionString)
        {
            var result = -1;
            SqlConnection cnn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            var stopwatch = new Stopwatch();

            try
            {
                cnn = GetConnection(connectionString);
                command = GetSqlCommand(cnn, statement, sqlCommandParameters);

                if (logSql)
                {
                    LogCommandDebug(command);
                    stopwatch.Start();
                }

                reader = command.ExecuteReader();

                if (reader.NextResult())
                {
                    if (reader.Read())
                    {
                        result = GetInt32Value(reader, 0, -2);
                    }
                }

                if (logSql)
                {
                    stopwatch.Stop();
                    Log.DebugFormat("2nd statement results: {0}, statement time: {1}ms", result,
                        stopwatch.ElapsedMilliseconds);
                }

                return result;
            }
            catch (Exception ex)
            {
                LogCommandInfo(command);
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                DisposeConnections(cnn, command, reader);
            }
        }

        public int ExecuteStatement(string statement, bool logSql, string connectionString)
        {
            SqlConnection cnn = null;
            SqlCommand command = null;

            var stopwatch = new Stopwatch();

            try
            {
                cnn = GetConnection(connectionString);
                command = GetSqlCommand(cnn, statement);

                if (logSql)
                {
                    LogCommandDebug(command);
                    stopwatch.Start();
                }

                var rows = command.ExecuteNonQuery();

                if (logSql)
                {
                    stopwatch.Stop();
                    Log.DebugFormat("rows effected: {0}, statement time: {1}ms", rows, stopwatch.ElapsedMilliseconds);
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

        public int ExecuteStatement(string statement, ISqlCommandParameter[] sqlCommandParameters, bool logSql,
            string connectionString)
        {
            SqlConnection cnn = null;
            SqlCommand command = null;

            var stopwatch = new Stopwatch();

            try
            {
                cnn = GetConnection(connectionString);
                command = GetSqlCommand(cnn, statement, sqlCommandParameters);

                if (logSql)
                {
                    LogCommandDebug(command);
                    stopwatch.Start();
                }

                var rows = command.ExecuteNonQuery();

                if (logSql)
                {
                    stopwatch.Stop();
                    Log.DebugFormat("rows effected: {0}, statement time: {1}ms", rows, stopwatch.ElapsedMilliseconds);
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

        public string RemoveControlCharacters(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var newValue = value.Replace(ControlCharacterStartOfHeader, string.Empty)
                .Replace(ControlCharacterStartOfText, string.Empty)
                .Replace(ControlCharacterEndOfText, string.Empty)
                .Replace(ControlCharacterEndOfTransmission, string.Empty)
                .Replace(ControlCharacterEnquiry, string.Empty)
                .Replace(ControlCharacterAcknowledgment, string.Empty)
                .Replace(ControlCharacterAudibleBell, string.Empty)
                .Replace(ControlCharacterBackspace, string.Empty)
                .Replace(ControlCharacterVerticalTab, string.Empty)
                .Replace(ControlCharacterFormFeed, string.Empty)
                .Replace(ControlCharacterShiftOut, string.Empty)
                .Replace(ControlCharacterShiftIn, string.Empty)
                .Replace(ControlCharacterDataLinkEscape, string.Empty)
                .Replace(ControlCharacterXon, string.Empty)
                .Replace(ControlCharacterDc2, string.Empty)
                .Replace(ControlCharacterXoff, string.Empty)
                .Replace(ControlCharacterDc4, string.Empty)
                .Replace(ControlCharacterNegativeAcknowledgment, string.Empty)
                .Replace(ControlCharacterSynchronousIdle, string.Empty)
                .Replace(ControlCharacterEndOfTransmissionBlock, string.Empty)
                .Replace(ControlCharacterCancel, string.Empty)
                .Replace(ControlCharacterEndOfMedium, string.Empty)
                .Replace(ControlCharacterSubstituteChar, string.Empty)
                .Replace(ControlCharacterEscape, string.Empty)
                .Replace(ControlCharacterFileSeparator, string.Empty)
                .Replace(ControlCharacterGroupSeparator, string.Empty)
                .Replace(ControlCharacterRecordSeparator, string.Empty)
                .Replace(ControlCharacterUnitSeparator, string.Empty)
                .Replace(ControlCharacterDelete, string.Empty);
            return newValue;
        }

        public int ExecuteBasicCountQuery(string sql, List<ISqlCommandParameter> sqlCommandParameters, bool logSql)
        {
            return ExecuteBasicCountQuery(sql, sqlCommandParameters, logSql, ConnectionString);
        }

        public int ExecuteBasicCountQuery(string sql, List<ISqlCommandParameter> sqlCommandParameters, bool logSql,
            string connectionString)
        {
            SqlConnection cnn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            var stopwatch = new Stopwatch();

            var count = -1;

            Log.DebugFormat("sql: {0}", sql);
            try
            {
                cnn = GetConnection(connectionString);
                command = GetSqlCommand(cnn, sql, sqlCommandParameters.ToArray());

                if (logSql)
                {
                    LogCommandDebug(command);
                    stopwatch.Start();
                }

                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    count = GetInt32Value(reader, 0, -2);
                }

                if (logSql)
                {
                    stopwatch.Stop();
                    Log.DebugFormat("count: {0}, select count time: {1}ms", count, stopwatch.ElapsedMilliseconds);
                }

                return count;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                DisposeConnections(cnn, command, reader);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="transaction"> </param>
        public int ExecuteBasicCountQuery(string sql, List<ISqlCommandParameter> sqlCommandParameters, bool logSql,
            SqlConnection cnn, SqlTransaction transaction)
        {
            SqlCommand command = null;
            SqlDataReader reader = null;

            var stopwatch = new Stopwatch();

            var count = -1;

            Log.DebugFormat("sql: {0}", sql);
            try
            {
                command = GetSqlCommand(cnn, sql, sqlCommandParameters.ToArray(), 15, transaction);

                if (logSql)
                {
                    LogCommandDebug(command);
                    stopwatch.Start();
                }

                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    count = GetInt32Value(reader, 0, -2);
                }

                if (logSql)
                {
                    stopwatch.Stop();
                    Log.DebugFormat("count: {0}, select count time: {1}ms", count, stopwatch.ElapsedMilliseconds);
                }

                return count;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                DisposeConnections(null, command, reader);
            }
        }
    }
}