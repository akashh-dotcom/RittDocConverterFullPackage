#region

using R2Library.Data.ADO.R2.DataServices;
using R2V2.WindowsService.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.DataServices
{
    public class LicensingDataService : LicensingDataServiceBase
    {
        public LicensingDataService(WindowsServiceSettings windowsServiceSettings)
            : base(windowsServiceSettings.RIT001ProductionConnectionString)
        {
        }
    }
}