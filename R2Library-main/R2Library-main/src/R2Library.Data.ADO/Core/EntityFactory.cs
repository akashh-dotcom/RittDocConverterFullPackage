#region

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using R2Library.Data.ADO.Core.SqlCommandParameters;

#endregion

namespace R2Library.Data.ADO.Core
{
    public class EntityFactory : FactoryBase
    {
        public T GetFirstEntity<T>(string selectStatement, List<ISqlCommandParameter> sqlCommandParameters, bool logSql)
            where T : IDataEntity, new()
        {
            //return GetFirstEntity<T>(selectStatement, sqlCommandParameters, logSql, _databaseConnectionString);
            return GetFirstEntity<T>(selectStatement, sqlCommandParameters, logSql, ConnectionString);
        }

        public T GetFirstEntity<T>(string selectStatement, List<ISqlCommandParameter> sqlCommandParameters, bool logSql,
            string connectionString) where T : IDataEntity, new()
        {
            SqlConnection cnn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            var stopwatch = new Stopwatch();

            var item = new T();

            try
            {
                cnn = GetConnection(connectionString);
                command = GetSqlCommand(cnn, selectStatement, sqlCommandParameters.ToArray());

                if (logSql)
                {
                    LogCommandDebug(command);
                    stopwatch.Start();
                }

                reader = command.ExecuteReader();

                //Log.DebugFormat("query time: {0}ms", stopwatch.ElapsedMilliseconds);
                if (reader.Read())
                {
                    //Log.DebugFormat("query time: {0}ms", stopwatch.ElapsedMilliseconds);
                    item.Populate(reader);
                    //Log.DebugFormat("query time: {0}ms", stopwatch.ElapsedMilliseconds);
                }

                if (logSql)
                {
                    stopwatch.Stop();
                    Log.DebugFormat("query time: {0}ms", stopwatch.ElapsedMilliseconds);
                }

                return item;
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

        public T GetFirstEntity<T>(string selectStatement, List<ISqlCommandParameter> sqlCommandParameters, bool logSql,
            SqlConnection cnn, SqlTransaction transaction) where T : IDataEntity, new()
        {
            //SqlConnection cnn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            var stopwatch = new Stopwatch();

            var item = new T();

            try
            {
                //cnn = GetConnection(connectionString);
                //command = GetSqlCommand(cnn, selectStatement, sqlCommandParameters.ToArray());
                command = GetSqlCommand(cnn, selectStatement, sqlCommandParameters, 15, transaction);

                if (logSql)
                {
                    LogCommandDebug(command);
                    stopwatch.Start();
                }

                reader = command.ExecuteReader();

                //Log.DebugFormat("query time: {0}ms", stopwatch.ElapsedMilliseconds);
                if (reader.Read())
                {
                    //Log.DebugFormat("query time: {0}ms", stopwatch.ElapsedMilliseconds);
                    item.Populate(reader);
                    //Log.DebugFormat("query time: {0}ms", stopwatch.ElapsedMilliseconds);
                }

                if (logSql)
                {
                    stopwatch.Stop();
                    Log.DebugFormat("query time: {0}ms", stopwatch.ElapsedMilliseconds);
                }

                return item;
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


        public List<T> GetEntityList<T>(string selectStatement, List<ISqlCommandParameter> sqlCommandParameters,
            bool logSql) where T : IDataEntity, new()
        {
            //return GetEntityList<T>(selectStatement, sqlCommandParameters, logSql, _databaseConnectionString);
            return GetEntityList<T>(selectStatement, sqlCommandParameters, logSql, ConnectionString);
        }

        public List<T> GetEntityList<T>(string selectStatement, List<ISqlCommandParameter> sqlCommandParameters,
            bool logSql, string connectionString) where T : IDataEntity, new()
        {
            SqlConnection cnn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            var stopwatch = new Stopwatch();

            var items = new List<T>();

            try
            {
                cnn = GetConnection(connectionString);

                command = sqlCommandParameters == null
                    ? GetSqlCommand(cnn, selectStatement)
                    : GetSqlCommand(cnn, selectStatement, sqlCommandParameters.ToArray());

                if (logSql)
                {
                    LogCommandDebug(command);
                    stopwatch.Start();
                }

                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var item = new T();
                    item.Populate(reader);
                    items.Add(item);
                }

                if (logSql)
                {
                    stopwatch.Stop();
                    Log.DebugFormat("query time: {0}ms, count: {1}", stopwatch.ElapsedMilliseconds, items.Count);
                }

                return items;
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
    }
}