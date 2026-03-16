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
    public class TermHighlightQueueDataService : DataServiceBase
    {
        public TermHighlightQueueDataService(TermHighlightType termHighlightType)
        {
            TermHighlightType = termHighlightType;
        }

        private string TableName
        {
            get
            {
                switch (TermHighlightType)
                {
                    case TermHighlightType.Tabers:
                        return "TabersTermHighlightQueue";
                    case TermHighlightType.IndexTerms:
                        return "IndexTermHighlightQueue";
                    default:
                        throw new Exception(
                            $"Unexpected TermHighlighQueueDataService error occurred - Unknown TermHighlightType: {TermHighlightType}");
                }
            }
        }

        private TermHighlightType TermHighlightType { get; }

        public IEnumerable<TermHighlightQueue> GetNextBatch(int batchSize)
        {
            var sql = string.Format(BatchSelectStatement, batchSize, TermHighlightType, TableName);

            IList<TermHighlightQueue> termHighlightQueues = GetEntityList<TermHighlightQueue>(sql, null, true);

            return termHighlightQueues;
        }

        public TermHighlightQueue GetNext(bool descending)
        {
            SqlConnection cnn = null;

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            SqlTransaction transaction = null;

            var sql = string.Format(BatchSelectStatement, 1, TermHighlightType, TableName, descending ? " desc" : "");

            try
            {
                cnn = GetConnection();
                transaction = cnn.BeginTransaction();

                var queue = GetFirstEntity<TermHighlightQueue>(sql, null, true, cnn, transaction);

                if (queue != null && queue.Id > 0)
                {
                    Log.DebugFormat("resourceId: {0}, isbn: {1}", queue.ResourceId, queue.Isbn);
                    queue.TermHighlightStatus = "P";
                    queue.DateStarted = DateTime.Now;
                    Update(queue);
                }
                else queue = null;

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

        public int GetTermHighlightQueueSize()
        {
            const string sql = "select count(*) from {0} where termHighlightStatus = @TermHighlightStatus";

            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("TermHighlightStatus", "A")
            };

            var count = ExecuteBasicCountQuery(string.Format(sql, TableName), parameters, true);
            return count;
        }

        public int Update(TermHighlightQueue termHighlightQueue)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("TermHighlightStatus", termHighlightQueue.TermHighlightStatus),
                new DateTimeNullParameter("DateStarted", termHighlightQueue.DateStarted),
                new DateTimeNullParameter("DateFinished", termHighlightQueue.DateFinished),
                new Int32Parameter("FirstDocumentId", termHighlightQueue.FirstDocumentId),
                new Int32Parameter("LastDocumentId", termHighlightQueue.LastDocumentId),
                new StringParameter("StatusMessage", termHighlightQueue.StatusMessage),
                new Int32Parameter("TermHighlightQueueId", termHighlightQueue.Id)
            };

            var rows = ExecuteUpdateStatement(string.Format(UpdateStatement, TableName), parameters.ToArray(), true);
            return rows;
        }

        protected int Insert(int resourceId, string isbn, string status, DateTime dateAdded)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("TermHighlightStatus", status),
                new DateTimeNullParameter("DateAdded", dateAdded),
                new Int32Parameter("ResourceId", resourceId),
                new StringParameter("Isbn", isbn)
            };

            var rows = ExecuteInsertStatementReturnIdentity(string.Format(InsertStatement, TableName),
                parameters.ToArray(), true);
            return rows;
        }

        public bool AddResourceToQueue(int resourceId, string isbn)
        {
            if (DoesResourceExistInQueue(resourceId, isbn))
            {
                Log.DebugFormat("Resource already exist in term highlight queue, resource id: {0}, isbn: {1}",
                    resourceId, isbn);
                return false;
            }

            var id = Insert(resourceId, isbn, "A", DateTime.Now);
            Log.DebugFormat("TermHighlightQueueId: {0}", id);
            return true;
        }

        public bool DoesResourceExistInQueue(int resourceId, string isbn)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("TermHighlightStatus", "A"),
                new Int32Parameter("ResourceId", resourceId),
                new StringParameter("Isbn", isbn)
            };
            var count = ExecuteBasicCountQuery(string.Format(ResourceCheckCountStatement, TableName), parameters, true);
            return count > 0;
        }

        #region Fields

        private readonly string UpdateStatement = new StringBuilder()
            .Append("update {0} ")
            .Append("set termHighlightStatus = @TermHighlightStatus ")
            .Append("  , dateStarted = @DateStarted ")
            .Append("  , dateFinished = @DateFinished ")
            .Append("  , firstDocumentId = @FirstDocumentId ")
            .Append("  , lastDocumentId = @LastDocumentId ")
            .Append("  , statusMessage = @StatusMessage ")
            .Append("where termHighlightQueueId = @TermHighlightQueueId ")
            .ToString();

        private readonly string BatchSelectStatement = new StringBuilder()
            .Append("select top {0} termHighlightQueueId, jobId, resourceId, isbn, termHighlightStatus, dateAdded, ")
            .Append(
                "       dateStarted, dateFinished, firstDocumentId, lastDocumentId, statusMessage, '{1}' termHighlightType ")
            .Append("from   {2} ")
            .Append("where  termHighlightStatus = 'A' ")
            .Append("order by dateAdded, termHighlightQueueId ")
            .ToString();

        private readonly string ResourceCheckCountStatement = new StringBuilder()
            .Append("select count(*) ")
            .Append("from   {0} ")
            .Append("where  termHighlightStatus = @TermHighlightStatus and resourceId = @ResourceId and isbn = @Isbn ")
            .ToString();

        private readonly string InsertStatement = new StringBuilder()
            .Append("insert into {0} (jobId, resourceId, isbn, termHighlightStatus, dateAdded) ")
            .Append("values (@ResourceId, @Isbn, @TermHighlightStatus, @DateAdded)")
            .ToString();

        #endregion Fields
    }
}