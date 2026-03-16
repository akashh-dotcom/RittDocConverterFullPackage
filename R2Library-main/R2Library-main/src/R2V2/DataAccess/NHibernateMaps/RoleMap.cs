#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class RoleMap : BaseMap<Role>
    {
        public RoleMap()
        {
            Table("dbo.tRole");
            Id(x => x.Id, "iRoleId").GeneratedBy.Identity();
            Map(x => x.Code, "vchRoleCode");
            Map(x => x.Description, "vchRoleDesc");
        }
    }
}