#region

using R2Library.Data.ADO.Config;
using R2Library.Data.ADO.Core;

#endregion

namespace R2Library.Data.ADO.R2.DataServices
{
    public class DataServiceBase : EntityFactory
    {
        public DataServiceBase()
        {
            ConnectionString = DbConfigSettings.Settings.R2DatabaseConnection;
        }

        public DataServiceBase(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}