#region

using R2Library.Data.ADO.R2.DataServices;
using R2Utilities.Infrastructure.Settings;

#endregion

namespace R2Utilities.DataAccess
{
    public class LicensingDataService : LicensingDataServiceBase
    {
        public LicensingDataService(IR2UtilitiesSettings r2UtilitiesSettings)
            : base(r2UtilitiesSettings.R2DatabaseConnection)
        {
        }
    }
}