namespace R2Library.Data.ADO.Config
{
    public class R2DbConfigSettings : IDbConfigSettings
    {
        /// <summary>
        /// </summary>
        public R2DbConfigSettings(string r2DatabaseConnection, string r2UtilitiesDatabaseConnection,
            string r2ReportsConnection)
        {
            R2DatabaseConnection = r2DatabaseConnection;
            R2UtilitiesDatabaseConnection = r2UtilitiesDatabaseConnection;
            R2ReportsConnection = r2ReportsConnection;
        }

        public string R2UtilitiesDatabaseConnection { get; }
        public string R2DatabaseConnection { get; }

        public string R2ReportsConnection { get; }
    }
}