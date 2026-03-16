#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.Core;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2V2.WindowsService.Entities;
using R2V2.WindowsService.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.DataServices
{
    public class ResourceUploadQueDataService : EntityFactory
    {
        public ResourceUploadQueDataService(WindowsServiceSettings windowsServiceSettings)
        {
            ConnectionString = windowsServiceSettings.RIT001ProductionConnectionString;
        }

        public int AddResourceUploadQueRecord(int resourceId, string isbn, string email)
        {
            const string sql = "select count(*) from tResourceUploadQue where vchResourceISBN = @Isbn;";

            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("Isbn", isbn)
            };

            var count = ExecuteBasicCountQuery(sql, parameters, true);

            if (count > 0)
            {
                return 0;
            }

            var insert = new StringBuilder()
                .Append(
                    "insert into tResourceUploadQue (iResourceId, vchResourceISBN, vchResponseEmailId, tiProcessed, iFinalStatus, vchFinalMessage ")
                .Append("    , vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) ")
                .Append(
                    "values (@ResourceId, @Isbn, @Email, 0, 0, @FinalMessage, 'R2Promote', getdate(), null, null, 1); ");

            var msg = $"Book {isbn} was successfully uploaded";

            parameters.Add(new Int32Parameter("ResourceId", resourceId));
            parameters.Add(new StringParameter("Email", email));
            parameters.Add(new StringParameter("FinalMessage", msg));

            var id = ExecuteInsertStatementReturnIdentity(insert.ToString(), parameters, true);
            return id;
        }

        public List<ResourceToPromote> GetOverlapingResourcesByIsbn(int resourceId)
        {
            try
            {
                var sql = new StringBuilder()
                    .Append(
                        "select r2.iResourceId, r2.vchResourceTitle, r2.vchResourceISBN, r2.vchResourceImageName, r2.tiRecordStatus, r2.vchIsbn10, r2.vchIsbn13, r2.vchEIsbn ")
                    .Append("from tResource r ")
                    .Append(
                        "join tResource r2 on (r.vchIsbn10 = r2.vchIsbn10 or r.vchIsbn10 = r2.vchIsbn13 or r.vchIsbn10 = r2.vchEIsbn) and r.iResourceId<> r2.iResourceId and r2.tiRecordStatus = 1 ")
                    .Append("where r.iResourceId = @ResourceId ")
                    .Append("union ")
                    .Append(
                        "select r2.iResourceId, r2.vchResourceTitle, r2.vchResourceISBN, r2.vchResourceImageName, r2.tiRecordStatus, r2.vchIsbn10, r2.vchIsbn13, r2.vchEIsbn ")
                    .Append("from tResource r ")
                    .Append(
                        "join tResource r2 on (r.vchIsbn13 = r2.vchIsbn10 or r.vchIsbn13 = r2.vchIsbn13 or r.vchIsbn13 = r2.vchEIsbn) and r.iResourceId<> r2.iResourceId and r2.tiRecordStatus = 1 ")
                    .Append("where r.iResourceId = @ResourceId ")
                    .Append("union ")
                    .Append(
                        "select r2.iResourceId, r2.vchResourceTitle, r2.vchResourceISBN, r2.vchResourceImageName, r2.tiRecordStatus, r2.vchIsbn10, r2.vchIsbn13, r2.vchEIsbn ")
                    .Append("from tResource r ")
                    .Append(
                        "join tResource r2 on (r.vchEIsbn = r2.vchIsbn10 or r.vchEIsbn = r2.vchIsbn13 or r.vchEIsbn = r2.vchEIsbn) and r.iResourceId<> r2.iResourceId and r2.tiRecordStatus = 1 ")
                    .Append("where r.iResourceId = @ResourceId ");

                var parameters = new List<ISqlCommandParameter>
                {
                    new Int32Parameter("ResourceId", resourceId)
                };

                IList<ResourceToPromote> list = GetEntityList<ResourceToPromote>(sql.ToString(), parameters, true);

                return list.ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }

            return null;
        }
    }
}