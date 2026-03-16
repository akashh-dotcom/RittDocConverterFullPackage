#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;

#endregion

namespace R2Library.Data.ADO.R2Utility.DataServices
{
    public class FixHtmlQueueDataService : DataServiceBase
    {
        private readonly string BatchSelectStatement = new StringBuilder()
            .Append("select top {0} fixHtmlQueueId, resourceId, isbn, status, dateAdded, ")
            .Append("       dateStarted, dateFinished, statusMessage ")
            .Append("from   FixHtmlQueue ")
            .Append("where  status = 'A' ")
            .Append("order by dateAdded, FixHtmlQueueId ")
            .ToString();

        private readonly string UpdateStatement = new StringBuilder()
            .Append("update FixHtmlQueue ")
            .Append("set status = @Status ")
            .Append("  , dateStarted = @DateStarted ")
            .Append("  , dateFinished = @DateFinished ")
            .Append("  , statusMessage = @StatusMessage ")
            .Append("where fixHtmlQueueId = @FixHtmlQueueId ")
            .ToString();

        public IEnumerable<FixHtmlQueue> GetNextBatch(int batchSize)
        {
            var sql = string.Format(BatchSelectStatement, batchSize);

            //EntityFactory entityFactory = new EntityFactory(_connectionString);
            //EntityFactory entityFactory = new EntityFactory();
            IList<FixHtmlQueue> queues = GetEntityList<FixHtmlQueue>(sql, null, true);

            return queues;
        }

        public FixHtmlQueue GetNext()
        {
            var sql = string.Format(BatchSelectStatement, 1);

            //EntityFactory entityFactory = new EntityFactory(_connectionString);
            //EntityFactory entityFactory = new EntityFactory();
            IList<FixHtmlQueue> queues = GetEntityList<FixHtmlQueue>(sql, null, true);

            if (queues.Count == 0)
            {
                return null;
            }

            return queues.First();
        }

        public int Update(FixHtmlQueue fixHtmlQueue)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("Status", fixHtmlQueue.Status),
                new DateTimeNullParameter("DateStarted", fixHtmlQueue.DateStarted),
                new DateTimeNullParameter("DateFinished", fixHtmlQueue.DateFinished),
                new StringParameter("StatusMessage", fixHtmlQueue.StatusMessage),
                new Int32Parameter("FixHtmlQueueId", fixHtmlQueue.Id)
            };

            var rows = ExecuteUpdateStatement(UpdateStatement, parameters.ToArray(), true, ConnectionString);
            return rows;
        }
    }
}