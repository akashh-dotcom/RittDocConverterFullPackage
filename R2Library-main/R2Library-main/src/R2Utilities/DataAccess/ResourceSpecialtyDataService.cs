#region

using System.Collections.Generic;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2Library.Data.ADO.R2.DataServices;

#endregion

namespace R2Utilities.DataAccess
{
    public class ResourceSpecialtyDataService : DataServiceBase
    {
        public IList<ResourceSpecialty> GetResourceSpecialty(int resourceId)
        {
            var sql = new StringBuilder()
                .Append("select s.iSpecialtyId, s.vchSpecialtyCode, s.vchSpecialtyName, s.tiRecordStatus ")
                .Append("from   tResourceSpecialty rs ")
                .Append(" join  dbo.tSpecialty s on s.iSpecialtyId = rs.iSpecialtyId ")
                .Append("where  rs.iResourceId = @ResourceId ");

            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("ResourceId", resourceId)
            };


            var list = GetEntityList<ResourceSpecialty>(sql.ToString(), parameters, true);
            return list;
        }

        public int Insert(int resourceId, string specialtyCode, string creatorId)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("SpecialtyCode", specialtyCode)
            };
            var insert = new StringBuilder()
                .Append(
                    "insert into tResourceSpecialty (iResourceId, iSpecialtyId, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) ")
                .AppendFormat("    select {0}, s.iSpecialtyId, '{1}', getdate(), null, null, 1 ", resourceId, creatorId)
                .Append("    from   tSpecialty s")
                .Append("    where  vchSpecialtyCode = @SpecialtyCode ");

            // SJS - 1/21/2014 - If you are not logging the SQL to the log4net logs, there better be a big log explanation for it and approval from the Pope or Dalai Lama.
            // I'm now spending my time correcting all of these calls where the logging was set to false.
            var rowCount = ExecuteUpdateStatement(insert.ToString(), parameters.ToArray(), true);
            Log.DebugFormat("insert row count: {0}", rowCount);
            return rowCount;
        }
    }
}