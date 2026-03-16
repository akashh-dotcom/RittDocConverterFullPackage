#region

using System;
using System.Collections.Generic;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;

#endregion

namespace R2Library.Data.ADO.R2Utility.DataServices
{
    public class IndexQueueDataService : DataServiceBase
    {
        private readonly string _batchSelectStatement = new StringBuilder()
            .Append("select top {0} indexQueueId, resourceId, isbn, indexStatus, dateAdded, ")
            .Append("       dateStarted, dateFinished, firstDocumentId, lastDocumentId, statusMessage ")
            .Append("from   IndexQueue ")
            .Append("where  indexStatus = 'A' and resourceId >= @MinResourceId and resourceId <= @MaxResourceId ")
            .Append("order by dateAdded, indexQueueId ")
            .ToString();

        private readonly string _insertStatement = new StringBuilder()
            .Append("insert into IndexQueue (resourceId, isbn, indexStatus, dateAdded) ")
            .Append("values (@ResourceId, @Isbn, @IndexStatus, @DateAdded)")
            .ToString();

        private readonly string _resourceCheckCountStatement = new StringBuilder()
            .Append("select count(*) ")
            .Append("from   IndexQueue ")
            .Append("where  indexStatus = @IndexStatus and resourceId = @ResourceId and isbn = @Isbn ")
            .ToString();

        private readonly string _updateStatement = new StringBuilder()
            .Append("update IndexQueue ")
            .Append("set indexStatus = @IndexStatus ")
            .Append("  , dateStarted = @DateStarted ")
            .Append("  , dateFinished = @DateFinished ")
            .Append("  , firstDocumentId = @FirstDocumentId ")
            .Append("  , lastDocumentId = @LastDocumentId ")
            .Append("  , statusMessage = @StatusMessage ")
            .Append("where indexQueueId = @IndexQueueId ")
            .ToString();

        public IEnumerable<IndexQueue> GetNextBatch(int batchSize, int minResourceId, int maxResourceId)
        {
            var sql = string.Format(_batchSelectStatement, batchSize);

            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("MinResourceId", minResourceId),
                new Int32Parameter("MaxResourceId", maxResourceId)
            };

            IList<IndexQueue> indexQueues = GetEntityList<IndexQueue>(sql, parameters, true);
            return indexQueues;
        }

        public int GetIndexQueueSize()
        {
            const string sql = "select count(*) from IndexQueue where indexStatus = @IndexStatus";

            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("IndexStatus", "A")
            };

            //int count = ExecuteBasicCountQuery(sql, parameters, true, _connectionString);
            var count = ExecuteBasicCountQuery(sql, parameters, true);
            return count;
        }

        public int Update(IndexQueue indexQueue)
        {
            // dateCompleted = @DateCompleted, successful = @Successful, results = @Results where transformedResourceId = @TransformedResourceId
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("IndexStatus", indexQueue.IndexStatus),
                new DateTimeNullParameter("DateStarted", indexQueue.DateStarted),
                new DateTimeNullParameter("DateFinished", indexQueue.DateFinished),
                new Int32Parameter("FirstDocumentId", indexQueue.FirstDocumentId),
                new Int32Parameter("LastDocumentId", indexQueue.LastDocumentId),
                new StringParameter("StatusMessage", indexQueue.StatusMessage),
                new Int32Parameter("IndexQueueId", indexQueue.Id)
            };

            //int rows = ExecuteUpdateStatement(UpdateStatement, parameters.ToArray(), true, _connectionString);
            var rows = ExecuteUpdateStatement(_updateStatement, parameters.ToArray(), true);
            return rows;
        }

        protected int Insert(int resourceId, string isbn, string status, DateTime dateAdded)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("IndexStatus", status),
                new DateTimeNullParameter("DateAdded", dateAdded),
                new Int32Parameter("ResourceId", resourceId),
                new StringParameter("Isbn", isbn)
            };

            var rows = ExecuteInsertStatementReturnIdentity(_insertStatement, parameters.ToArray(), true);
            //int rows = ExecuteInsertStatementReturnIdentity(InsertStatement, parameters.ToArray(), true, _connectionString);
            return rows;
        }

        public bool AddResourceToQueue(int resourceId, string isbn)
        {
            if (DoesResourceExistInQueue(resourceId, isbn))
            {
                Log.DebugFormat("Resource already exist in index queue, resource id: {0}, isbn: {1}", resourceId, isbn);
                return false;
            }

            var id = Insert(resourceId, isbn, "A", DateTime.Now);
            Log.DebugFormat("IndexQueueId: {0}", id);
            return true;
        }

        public bool DoesResourceExistInQueue(int resourceId, string isbn)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("IndexStatus", "A"),
                new Int32Parameter("ResourceId", resourceId),
                new StringParameter("Isbn", isbn)
            };
            var count = ExecuteBasicCountQuery(_resourceCheckCountStatement, parameters, true);
            return count > 0;
        }


        public IEnumerable<IndexQueue> GetForthcomingResourcesToIndex(string r2DatabaseName)
        {
            var sql = $@"
select iq.indexQueueId, iq.resourceId, iq.isbn, iq.indexStatus, iq.dateAdded,
iq.dateStarted, iq.dateFinished, iq.firstDocumentId, iq.lastDocumentId, iq.statusMessage
from   IndexQueue iq
join	{r2DatabaseName}..tResource r on iq.resourceId = r.iResourceId and r.iResourceStatusId = 8
where  iq.indexStatus = 'A'
order by iq.dateAdded, iq.indexQueueId
";
            IList<IndexQueue> indexQueues = GetEntityList<IndexQueue>(sql, new List<ISqlCommandParameter>(), true);
            return indexQueues;
        }
    }
}