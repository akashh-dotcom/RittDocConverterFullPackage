#region

using R2V2.Core.Admin;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class AdministratorAlertMap : BaseMap<AdministratorAlert>
    {
        public AdministratorAlertMap()
        {
            Table("tAlert");

            Id(x => x.Id).Column("iAlertId").GeneratedBy.Identity();
            Map(x => x.Title).Column("vchTitle");
            Map(x => x.Text).Column("vchText").CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.DisplayOnce).Column("tiDisplayOnce");

            Map(x => x.Layout).Column("iLayoutType").CustomType<AlertLayout>();

            Map(x => x.StartDate).Column("dtStartDate");
            Map(x => x.EndDate).Column("dtEndDate");

            Map(x => x.AlertName).Column("vchAlertName");

            References(x => x.Role).Column("iRoleId").ReadOnly().Cascade.None();
            Map(x => x.RoleId).Column("iRoleId");

            Map(x => x.ResourceId).Column("iResourceId");
            Map(x => x.AllowPDA).Column("tiAllowPDA");
            Map(x => x.AllowPurchase).Column("tiAllowPurchase");

            HasMany(x => x.AlertImages).KeyColumn("iAlertId").ReadOnly().Cascade.None();
        }
    }
}