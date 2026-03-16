#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;

#endregion

namespace R2Library.Data.ADO.R2Utility.DataServices
{
    public class FixDocIdsQueueDataService : DataServiceBase
    {
        private readonly string _batchSelectStatement = new StringBuilder()
            .Append("select top {0} fixDocIdsQueueId, resourceId, isbn, status, dateAdded, ")
            .Append("       dateStarted, dateFinished, statusMessage ")
            .Append("from   FixDocIdsQueue  ")
            .Append("where  status = 'A' ")
            .Append("order by dateAdded, fixDocIdsQueueId ")
            .ToString();

        private readonly string _updateStatement = new StringBuilder()
            .Append("update FixDocIdsQueue ")
            .Append("set status = @Status ")
            .Append("  , dateStarted = @DateStarted ")
            .Append("  , dateFinished = @DateFinished ")
            .Append("  , statusMessage = @StatusMessage ")
            .Append("where fixDocIdsQueueId = @FixDocIdsQueueId ")
            .ToString();

        public IEnumerable<FixDocIdsQueue> GetNextBatch(int batchSize)
        {
            var sql = string.Format(_batchSelectStatement, batchSize);

            //EntityFactory entityFactory = new EntityFactory(_connectionString);
            //EntityFactory entityFactory = new EntityFactory();
            IList<FixDocIdsQueue> queues = GetEntityList<FixDocIdsQueue>(sql, null, true);

            return queues;
        }

        public FixDocIdsQueue GetNext()
        {
            var sql = string.Format(_batchSelectStatement, 1);

            //EntityFactory entityFactory = new EntityFactory(_connectionString);
            //EntityFactory entityFactory = new EntityFactory();
            IList<FixDocIdsQueue> queues = GetEntityList<FixDocIdsQueue>(sql, null, true);

            if (queues.Count == 0)
            {
                return null;
            }

            return queues.First();
        }

        public int Update(FixDocIdsQueue fixDocIdsQueue)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("Status", fixDocIdsQueue.Status),
                new DateTimeNullParameter("DateStarted", fixDocIdsQueue.DateStarted),
                new DateTimeNullParameter("DateFinished", fixDocIdsQueue.DateFinished),
                new StringParameter("StatusMessage", fixDocIdsQueue.StatusMessage),
                new Int32Parameter("FixDocIdsQueueId", fixDocIdsQueue.Id)
            };

            //int rows = ExecuteUpdateStatement(UpdateStatement, parameters.ToArray(), true, _connectionString);
            var rows = ExecuteUpdateStatement(_updateStatement, parameters.ToArray(), true);
            return rows;
        }
    }
}