namespace R2Library.Data.ADO.Config
{
    public interface IDbConfigSettings
    {
        string R2UtilitiesDatabaseConnection { get; }

        string R2DatabaseConnection { get; }

        string R2ReportsConnection { get; }
    }
}