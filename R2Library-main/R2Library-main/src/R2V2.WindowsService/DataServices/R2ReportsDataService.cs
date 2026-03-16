#region

using R2Library.Data.ADO.Core;
using R2V2.WindowsService.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.DataServices
{
    public class R2ReportsDataService : EntityFactory
    {
        public R2ReportsDataService(WindowsServiceSettings windowsServiceSettings)
        {
            ConnectionString = windowsServiceSettings.R2ReportsConnectionString;
        }
    }
}