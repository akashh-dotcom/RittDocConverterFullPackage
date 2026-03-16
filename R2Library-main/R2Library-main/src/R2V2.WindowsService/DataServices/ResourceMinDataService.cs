#region

using System.Collections.Generic;
using System.Text;
using R2Library.Data.ADO.Core;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2V2.Infrastructure.Logging;
using R2V2.WindowsService.Entities;
using R2V2.WindowsService.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.DataServices
{
    public class ResourceMinDataService : EntityFactory
    {
        private readonly ILog<PromoteDataService> _log;

        public ResourceMinDataService(ILog<PromoteDataService> log, WindowsServiceSettings windowsServiceSettings)
        {
            _log = log;
            ConnectionString = windowsServiceSettings.RIT001ProductionConnectionString;
        }

        public ResourceMin GetResourceMinByIsbn(string isbn)
        {
            var sql = new StringBuilder()
                .Append("select r.iResourceId, r.vchResourceTitle, r.vchResourceSubTitle, r.vchResourceAuthors ")
                .Append(
                    "     , r.dtRISReleaseDate, r.dtResourcePublicationDate, r.vchResourceISBN, r.vchResourceEdition ")
                .Append("     , r.vchCopyRight, r.iPublisherId, r.iResourceStatusId, r.tiRecordStatus ")
                .Append("     , r.vchResourceSortTitle, r.chrAlphaKey, r.vchIsbn10, r.vchIsbn13, r.vchEIsbn ")
                .Append("     , p.vchPublisherName ")
                .Append("from   dbo.tResource r ")
                .Append(" join  dbo.tPublisher p on p.iPublisherId = r.iPublisherId ")
                .Append("where  r.vchResourceISBN = @Isbn ");

            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("Isbn", isbn)
            };

            var resource = GetFirstEntity<ResourceMin>(sql.ToString(), parameters, true);

            return resource;
        }

        public int UpdateResourceStatus(int resourceId, int statusId)
        {
            var update = new StringBuilder()
                .Append("update tResource ")
                .Append(
                    " set   iResourceStatusId = @StatusId, vchUpdaterId = 'PromoteError', dtLastUpdate = getdate() ")
                .Append("where  iResourceId = @ResourceId ");

            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("StatusId", statusId),
                new Int32Parameter("ResourceId", resourceId)
            };
            var rowCount = ExecuteUpdateStatement(update.ToString(), parameters.ToArray(), true);
            _log.DebugFormat("update row count: {0}", rowCount);
            return rowCount;
        }
    }
}