#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.Core;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2V2.Infrastructure.Logging;
using R2V2.WindowsService.Entities;
using R2V2.WindowsService.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.DataServices
{
    public class ResourceToPromoteDataService : EntityFactory
    {
        private readonly ILog<PromoteDataService> _log;

        public ResourceToPromoteDataService(ILog<PromoteDataService> log, WindowsServiceSettings windowsServiceSettings)
        {
            _log = log;
            ConnectionString = windowsServiceSettings.RIT001StagingConnectionString;
        }

        public ResourceToPromote GetResourceToPromote(int resourceId, string isbn)
        {
            var sql = new StringBuilder()
                .Append(
                    "select r.iResourceId, r.vchResourceTitle, r.vchResourceISBN, r.vchResourceImageName, r.tiRecordStatus, r.vchIsbn10, r.vchIsbn13, r.vchEIsbn ")
                .Append("from   tResource r ")
                .Append("where  r.iResourceId = @ResourceId and r.vchResourceISBN = @Isbn;");

            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("Isbn", isbn),
                new Int32Parameter("ResourceId", resourceId)
            };
            IList<ResourceToPromote> list = GetEntityList<ResourceToPromote>(sql.ToString(), parameters, true);

            return list.FirstOrDefault();
        }
    }
}