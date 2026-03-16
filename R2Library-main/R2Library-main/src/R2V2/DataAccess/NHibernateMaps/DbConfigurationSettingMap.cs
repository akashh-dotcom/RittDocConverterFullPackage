#region

using R2V2.Core.Configuration;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public class DbConfigurationSettingMap : BaseMap<DbConfigurationSetting>
    {
        public DbConfigurationSettingMap()
        {
            Table("tConfigurationSetting");

            Id(x => x.Id).Column("iConfigurationSettingId").GeneratedBy.Identity();
            Map(x => x.Configuration).Column("vchConfiguration");
            Map(x => x.Setting).Column("vchSetting");
            Map(x => x.Key).Column("vchKey");
            Map(x => x.Value).Column("vchValue").CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.Description).Column("vchInstructions");
        }
    }
}