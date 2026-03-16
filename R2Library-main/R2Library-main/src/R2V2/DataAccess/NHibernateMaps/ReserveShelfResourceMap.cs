#region

using R2V2.Core.ReserveShelf;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ReserveShelfResourceMap : BaseMap<ReserveShelfResource>
    {
        public ReserveShelfResourceMap()
        {
            Table("tReserveListResource");

            Id(x => x.Id).Column("iReserveListResourceId").GeneratedBy.Identity();

            Map(x => x.ReserveShelfListId).Column("iReserveListId");
            Map(x => x.ResourceId).Column("iResourceId");

            //References(x => x.InstitutionResource).Column("iInstitutionResourceId");
        }
    }
}