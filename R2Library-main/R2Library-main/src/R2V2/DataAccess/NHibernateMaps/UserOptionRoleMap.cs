#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserOptionRoleMap : BaseMap<UserOptionRole>
    {
        public UserOptionRoleMap()
        {
            Table("dbo.tUserOptionRole");
            Id(x => x.Id, "iUserOptionRoleId").GeneratedBy.Identity();
            Map(x => x.DefaultValue, "vchDefaultValue");
            References(x => x.Role, "iRoleId").ReadOnly();
            References(x => x.Option).Column("iUserOptionId").ReadOnly();
        }
    }
}