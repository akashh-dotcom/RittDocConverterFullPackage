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
    public class TransformedResourceDataService : DataServiceBase
    {
        private const string SelectMinStatement = "select min(resourceId) from NonTransformedResourceId; ";


        //private const string InsertStatement =
        //    "insert into TransformedResource (resourceId, isbn, dateStarted, dateCompleted, successful, results) values(@ResourceId, @Isbn, @DateStarted, @DateCompleted, @Successful, @Results);";
        private const string InsertStatement =
            "insert into TransformedResource (resourceId, isbn, dateStarted, dateCompleted, successful, results) values(@ResourceId, @Isbn, @DateStarted, @DateCompleted, @Successful, @Results);";

        private const string UpdateStatement =
            "update TransformedResource set dateCompleted = @DateCompleted, successful = @Successful, results = @Results where transformedResourceId = @TransformedResourceId;";

        private static readonly object objectForLock = new object();

        private readonly string InsertSelectStatement = new StringBuilder()
            .Append("insert into TransformedResource (resourceId, isbn, successful) ")
            .Append("    select resourceId, isbn, 0 from NonTransformedResourceId where resourceId = @ResourceId ")
            .ToString();

        public int GetNextResourceIdToTransform(out int transformedResourceId)
        {
            Log.Debug("waiting for lock");
            lock (objectForLock)
            {
                SqlConnection cnn = null;

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                SqlTransaction transaction = null;
                try
                {
                    cnn = GetConnection();
                    transaction = cnn.BeginTransaction();

                    var resourceId = ExecuteBasicCountQuery(SelectMinStatement, new List<ISqlCommandParameter>(), false,
                        cnn, transaction);
                    Log.DebugFormat("resourceId: {0}", resourceId);

                    transformedResourceId = -1;
                    if (resourceId > 0)
                    {
                        transformedResourceId = Insert(resourceId, cnn, transaction);
                    }

                    transaction.Commit();

                    stopWatch.Stop();
                    Log.DebugFormat("next resourceId: {0}, transformedResourceId: {1}, query time: {2}", resourceId,
                        transformedResourceId,
                        stopWatch.ElapsedMilliseconds);
                    return resourceId;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                    Log.Warn("rolling back transaction");
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }

                    throw;
                }
                finally
                {
                    DisposeConnections(cnn);
                }
            }
        }

        public int Insert(TransformedResource transformedResource)
        {
            // @ResourceId, @Isbn, @DateCompleted, @Successful, @Results
            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("ResourceId", transformedResource.ResourceId),
                new StringParameter("Isbn", transformedResource.Isbn),
                new DateTimeParameter("DateStarted", transformedResource.DateStarted),
                new DateTimeNullParameter("DateCompleted", transformedResource.DateCompleted),
                new BooleanParameter("Successful", transformedResource.Successfully),
                new StringParameter("Results", transformedResource.Results)
            };

            transformedResource.Id = ExecuteInsertStatementReturnIdentity(InsertStatement, parameters, false);
            return transformedResource.Id;
        }

        /// <param name="cnn"> </param>
        /// <param name="transaction"> </param>
        public int Insert(int resourceId, SqlConnection cnn, SqlTransaction transaction)
        {
            // @ResourceId, @Isbn, @DateCompleted, @Successful, @Results
            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("ResourceId", resourceId)
            };

            var id = ExecuteInsertStatementReturnIdentity(InsertSelectStatement, parameters.ToArray(), false, cnn,
                transaction);
            return id;
        }

        public int Update(TransformedResource transformedResource)
        {
            // dateCompleted = @DateCompleted, successful = @Successful, results = @Results where transformedResourceId = @TransformedResourceId
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("Results", transformedResource.Results),
                new BooleanParameter("Successful", transformedResource.Successfully),
                new DateTimeNullParameter("DateCompleted", transformedResource.DateCompleted),
                new Int32Parameter("TransformedResourceId", transformedResource.Id)
            };

            var rows = ExecuteUpdateStatement(UpdateStatement, parameters, true);
            return rows;
        }

        public int GetNonTransformedResourceCount()
        {
            var count = ExecuteBasicCountQuery("select count(*) from NonTransformedResourceId;",
                new List<ISqlCommandParameter>(), false);
            return count;
        }
    }
}