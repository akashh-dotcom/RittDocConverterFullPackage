#region

using R2Library.Data.ADO.Config;

#endregion

namespace R2Utilities
{
    public class R2ReportsBase : R2UtilitiesBase
    {
        public R2ReportsBase()
        {
            ConnectionString = DbConfigSettings.Settings.R2ReportsConnection;
        }
    }
}