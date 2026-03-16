#region

using R2Library.Data.ADO.Config;
using R2Library.Data.ADO.Core;

#endregion

namespace R2Library.Data.ADO.R2Utility.DataServices
{
    public class DataServiceBase : EntityFactory
    {
        public DataServiceBase()
        {
            ConnectionString = DbConfigSettings.Settings.R2UtilitiesDatabaseConnection;
        }
    }
}