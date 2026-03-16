#region

using R2V2.Core.Institution;
using R2V2.Core.ReserveShelf;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ReserveShelfMap : BaseMap<ReserveShelf>
    {
        public ReserveShelfMap()
        {
            Table("tReserveList");

            Id(x => x.Id).Column("iReserveListId").GeneratedBy.Identity();

            Map(x => x.Name).Column("vchReserveListName");
            Map(x => x.Description).Column("vchReserveListDesc");
            Map(x => x.DefaultSortBy).Column("vchDefaultSortBy");
            Map(x => x.IsAscending).Column("tiIsAscending");

            Map(x => x.LibraryLocation).Column("iLibraryLocationId");

            References<Institution>(x => x.Institution).Column("iInstitutionId");

            HasMany(x => x.ReserveShelfResources).KeyColumn("iReserveListId").AsBag().Inverse().Cascade
                .AllDeleteOrphan().ApplyFilter<SoftDeleteFilter>();

            HasMany(x => x.ReserveShelfUrls).KeyColumn("iReserveListId").AsBag().Inverse().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
        }
    }
}