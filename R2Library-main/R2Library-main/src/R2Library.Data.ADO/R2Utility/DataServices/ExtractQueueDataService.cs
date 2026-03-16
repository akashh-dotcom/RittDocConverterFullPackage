#region

using System.Collections.Generic;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;

#endregion

namespace R2Library.Data.ADO.R2Utility.DataServices
{
    public class ExtractQueueDataService : DataServiceBase
    {
        private readonly string _batchSelectStatement = new StringBuilder()
            .Append("select top {0} extractQueueId, resourceId, isbn, status, dateAdded, ")
            .Append("       dateStarted, dateFinished, statusMessage ")
            .Append("from   ExtractQueue ")
            .Append("where  status = 'A' ")
            .Append("order by dateAdded, extractQueueId ")
            .ToString();

        private readonly string _updateStatement = new StringBuilder()
            .Append("update ExtractQueue ")
            .Append("set status = @Status ")
            .Append("  , dateStarted = @DateStarted ")
            .Append("  , dateFinished = @DateFinished ")
            .Append("  , statusMessage = @StatusMessage ")
            .Append("where extractQueueId = @extractQueueId ")
            .ToString();

        public IEnumerable<ExtractQueue> GetNextBatch(int batchSize)
        {
            var sql = string.Format(_batchSelectStatement, batchSize);
            IList<ExtractQueue> queues = GetEntityList<ExtractQueue>(sql, null, true);
            return queues;
        }

        public int GetQueueSize()
        {
            const string sql = "select count(*) from ExtractQueue where status = @Status";
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("Status", "A")
            };
            var count = ExecuteBasicCountQuery(sql, parameters, true);
            return count;
        }

        public int Update(ExtractQueue queue)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("Status", queue.Status),
                new DateTimeNullParameter("DateStarted", queue.DateStarted),
                new DateTimeNullParameter("DateFinished", queue.DateFinished),
                new StringParameter("StatusMessage", queue.StatusMessage),
                new Int32Parameter("extractQueueId", queue.Id)
            };

            var rows = ExecuteUpdateStatement(_updateStatement, parameters.ToArray(), true);
            return rows;
        }
    }
}