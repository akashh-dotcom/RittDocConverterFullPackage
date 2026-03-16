#region

using R2V2.Core.ReserveShelf;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ReserveShelfUrlMap : BaseMap<ReserveShelfUrl>
    {
        public ReserveShelfUrlMap()
        {
            Table("tExternalURL");

            Id(x => x.Id).Column("iExternalURLId").GeneratedBy.Identity();

            Map(x => x.Url).Column("vchURL");

            Map(x => x.Description).Column("vchExternalDesc");

            Map(x => x.ReserveShelfId).Column("iReserveListId");
        }
    }
}