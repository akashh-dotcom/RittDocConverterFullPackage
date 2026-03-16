#region

using System.Collections.Generic;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2Library.Data.ADO.R2.DataServices;

#endregion

namespace R2Utilities.DataAccess
{
    public class ResourcePracticeAreaDataService : DataServiceBase
    {
        public IList<ResourcePracticeArea> GetResourcePracticeArea(int resourceId)
        {
            // sjs - 10/16/2015 - added check to make sure the tResourcePracticeArea record and the tPracticeArea record has not been soft deleted
            var sql = new StringBuilder()
                .Append("select pa.iPracticeAreaId, pa.vchPracticeAreaCode, pa.vchPracticeAreaName, pa.tiRecordStatus ")
                .Append("from   tResourcePracticeArea rpa ")
                .Append(
                    " join  dbo.tPracticeArea pa on pa.iPracticeAreaId = rpa.iPracticeAreaId and pa.tiRecordStatus = 1 ")
                .Append("where  rpa.iResourceId = @ResourceId and rpa.tiRecordStatus = 1 ");

            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("ResourceId", resourceId)
            };

            var list = GetEntityList<ResourcePracticeArea>(sql.ToString(), parameters, true);
            return list;
        }

        public int Insert(int resourceId, string practiceAreaCode, string creatorId)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("PracticeAreaCode", practiceAreaCode)
            };
            var insert = new StringBuilder()
                .Append(
                    "insert into tResourcePracticeArea (iResourceId, iPracticeAreaId, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) ")
                .AppendFormat("    select {0}, pa.iPracticeAreaId, '{1}', getdate(), null, null, 1 ", resourceId,
                    creatorId)
                .Append("    from   tPracticeArea pa ")
                .Append("    where  vchPracticeAreaCode = @PracticeAreaCode ");

            // SJS - 1/21/2014 - If you are not logging the SQL to the log4net logs, there better be a big log explanation for it and approval from the Pope or Dalai Lama.
            // I'm now spending my time correcting all of these calls where the logging was set to false.
            var rowCount = ExecuteUpdateStatement(insert.ToString(), parameters.ToArray(), true);
            Log.DebugFormat("insert row count: {0}", rowCount);
            return rowCount;
        }
    }
}