#region

using System.Collections.Generic;
using System.IO;
using R2Library.Data.ADO.Core;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2V2.Infrastructure.Logging;
using R2V2.WindowsService.Entities;
using R2V2.WindowsService.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.DataServices
{
    public class PromoteDataService : EntityFactory
    {
        private readonly ILog<PromoteDataService> _log;
        private readonly WindowsServiceSettings _windowsServiceSettings;

        public PromoteDataService(ILog<PromoteDataService> log, WindowsServiceSettings windowsServiceSettings)
        {
            _log = log;
            _windowsServiceSettings = windowsServiceSettings;
            ConnectionString = windowsServiceSettings.RIT001StagingConnectionString;
        }

        public IList<Promote> PromoteResource(string isbn, string destinationDatabaseName)
        {
            var sql = GetPromotionScript(isbn, destinationDatabaseName);
            IList<Promote> promoteList = GetEntityList<Promote>(sql, null, true);
            return promoteList;
        }

        private string GetPromotionScript(string isbn, string destinationDatabaseName)
        {
            _log.DebugFormat("sql: {0}", _windowsServiceSettings.PromoteSqlScriptFile);
            var sql = File.ReadAllText(_windowsServiceSettings.PromoteSqlScriptFile);

            sql = sql.Replace("set @isbn = '1608316300'", $"set @isbn = '{isbn}'")
                .Replace("RIT001_2012-08-22", destinationDatabaseName);

            _log.DebugFormat("sql: {0}", sql);
            return sql;
        }

        public bool SetResourceLastPromotionDate(int resourceId)
        {
            var updateStatement =
                "update tResource set dtLastPromotionDate = getdate(), vchUpdaterId = 'R2Promote', dtLastUpdate = getdate() where iResourceId = @ResourceId";

            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("ResourceId", resourceId)
            };

            var rows = ExecuteUpdateStatement(updateStatement, parameters, true);
            return rows == 1;
        }
    }
}