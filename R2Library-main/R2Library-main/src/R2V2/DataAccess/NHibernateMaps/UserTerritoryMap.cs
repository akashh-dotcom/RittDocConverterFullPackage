#region

using R2V2.Core.Territory;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserTerritoryMap : BaseMap<UserTerritory>
    {
        public UserTerritoryMap()
        {
            Table("tUserTerritory");

            Id(x => x.Id).Column("iUserTerritoryId").GeneratedBy.Identity();

            Map(x => x.UserId).Column("iUserId");
            Map(x => x.TerritoryId).Column("iTerritoryId");

            References(x => x.User).Column("iUserId").ReadOnly().Cascade.None();
        }
    }
}