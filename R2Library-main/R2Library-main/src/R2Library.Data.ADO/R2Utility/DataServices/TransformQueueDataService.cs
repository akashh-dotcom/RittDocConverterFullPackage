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
    public class TransformQueueDataService : DataServiceBase
    {
        private readonly string _batchSelectStatement = new StringBuilder()
            .Append("select top {0} transformQueueId, resourceId, isbn, status, dateAdded, ")
            .Append("       dateStarted, dateFinished, statusMessage ")
            .Append("from   TransformQueue ")
            .Append("where  status = 'A' and resourceId >= @MinResourceId and resourceId <= @MaxResourceId ")
            .Append("order by dateAdded{1}, transformQueueId ")
            .ToString();

        private readonly string _countSelectStatement = new StringBuilder()
            .Append("select count(*) ")
            .Append("from   TransformQueue ")
            .Append("where  status = @Status and resourceId = @ResourceId and isbn = @Isbn ")
            .ToString();

        private readonly string _insertStatement = new StringBuilder()
            .Append("insert into TransformQueue (resourceId, isbn, status, dateAdded) ")
            .Append("values (@ResourceId, @Isbn, @Status, @DateAdded)")
            .ToString();

        private readonly string _updateStatement = new StringBuilder()
            .Append("update TransformQueue ")
            .Append("set status = @Status ")
            .Append("  , dateStarted = @DateStarted ")
            .Append("  , dateFinished = @DateFinished ")
            .Append("  , statusMessage = @StatusMessage ")
            .Append("where transformQueueId = @Id ")
            .ToString();

        /// <summary>
        /// </summary>
        /// <param name="descending"> </param>
        public IEnumerable<TransformQueue> GetNextBatch(int batchSize, bool descending, int minResourceId,
            int maxResourceId)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("MinResourceId", minResourceId),
                new Int32Parameter("MaxResourceId", maxResourceId)
            };

            var sql = string.Format(_batchSelectStatement, batchSize, descending ? " desc" : "");
            IList<TransformQueue> queues = GetEntityList<TransformQueue>(sql, parameters, true);
            return queues;
        }

        /// <summary>
        /// </summary>
        public TransformQueue GetNext(bool descending, int minResourceId, int maxResourceId)
        {
            SqlConnection cnn = null;

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            SqlTransaction transaction = null;

            var sql = string.Format(_batchSelectStatement, 1, descending ? " desc" : "");

            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("MinResourceId", minResourceId),
                new Int32Parameter("MaxResourceId", maxResourceId)
            };
            try
            {
                cnn = GetConnection();
                transaction = cnn.BeginTransaction();

                var queue = GetFirstEntity<TransformQueue>(sql, parameters, true, cnn, transaction);

                if (queue != null)
                {
                    Log.DebugFormat("resourceId: {0}, isbn: {1}", queue.ResourceId, queue.Isbn);
                    queue.Status = "P";
                    queue.DateStarted = DateTime.Now;
                    Update(queue);
                }

                transaction.Commit();

                stopWatch.Stop();
                Log.DebugFormat("query time: {0}", stopWatch.ElapsedMilliseconds);
                return queue;
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

        public int GetQueueSize()
        {
            const string sql = "select count(*) from TransformQueue where status = @Status";
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("Status", "A")
            };
            var count = ExecuteBasicCountQuery(sql, parameters, true);
            return count;
        }

        public int Update(TransformQueue queue)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("Status", queue.Status),
                new DateTimeNullParameter("DateStarted", queue.DateStarted),
                new DateTimeNullParameter("DateFinished", queue.DateFinished),
                new StringParameter("StatusMessage", queue.StatusMessage),
                new Int32Parameter("Id", queue.Id)
            };

            var rows = ExecuteUpdateStatement(_updateStatement, parameters.ToArray(), true);
            return rows;
        }

        public int Update(TransformQueue queue, SqlConnection cnn, SqlTransaction transaction)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("Status", queue.Status),
                new DateTimeNullParameter("DateStarted", queue.DateStarted),
                new DateTimeNullParameter("DateFinished", queue.DateFinished),
                new StringParameter("StatusMessage", queue.StatusMessage),
                new Int32Parameter("Id", queue.Id)
            };

            var rows = ExecuteUpdateStatement(_updateStatement, parameters.ToArray(), true, cnn, transaction);
            return rows;
        }


        public int Insert(int resourceId, string isbn, string status)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("Status", status),
                new DateTimeNullParameter("DateAdded", DateTime.Now),
                new Int32Parameter("ResourceId", resourceId),
                new StringParameter("Isbn", isbn)
            };

            var rows = ExecuteInsertStatementReturnIdentity(_insertStatement, parameters.ToArray(), true);
            return rows;
        }

        public int GetCount(int resourceId, string isbn, string status)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("Status", status),
                new DateTimeNullParameter("DateAdded", DateTime.Now),
                new Int32Parameter("ResourceId", resourceId),
                new StringParameter("Isbn", isbn)
            };

            var count = ExecuteBasicCountQuery(_countSelectStatement, parameters, true);
            return count;
        }
    }
}