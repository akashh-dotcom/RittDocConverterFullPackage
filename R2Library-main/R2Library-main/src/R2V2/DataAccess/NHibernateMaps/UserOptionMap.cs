#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserOptionMap : BaseMap<UserOption>
    {
        public UserOptionMap()
        {
            Table("dbo.tUserOption");
            Id(x => x.Id, "iUserOptionId").GeneratedBy.Identity();
            Map(x => x.Code, "vchUserOptionCode");
            Map(x => x.Description, "vchUserOptionDescription");
            References(x => x.Type).Column("iUserOptionTypeId").ReadOnly();
        }
    }
}