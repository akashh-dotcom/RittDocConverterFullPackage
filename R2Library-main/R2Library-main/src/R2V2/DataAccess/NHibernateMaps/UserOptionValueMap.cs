#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserOptionValueMap : BaseMap<UserOptionValue>
    {
        public UserOptionValueMap()
        {
            Table("dbo.tUserOptionValue");
            Id(x => x.Id, "iUserOptionValueId").GeneratedBy.Identity();
            Map(x => x.Value, "vchUserOptionValue");
            Map(x => x.UserId, "iUserId");
            Map(x => x.UserOptionId, "iUserOptionId");
            References(x => x.Option).Column("iUserOptionId").ReadOnly();
        }
    }
}