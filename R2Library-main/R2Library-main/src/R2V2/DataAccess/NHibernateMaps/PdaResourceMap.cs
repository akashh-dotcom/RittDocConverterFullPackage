#region

using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class PdaResourceMap : BaseMap<PdaResource>
    {
        public PdaResourceMap()
        {
            Table("tInstitutionPdaResource");
            Id(x => x.Id).Column("iInstitutionPdaResourceId").GeneratedBy.Identity();
            Map(x => x.InstitutionId, "iInstitutionId");
            Map(x => x.ResourceId, "iResourceId");
            Map(x => x.AddedToCartDate, "dtAddedToCartDate");
            Map(x => x.AddedToCartById, "vchAddedToCartById");
            Map(x => x.ViewCount, "iViewCount");
            Map(x => x.MaxViews, "iMaxViews");
        }
    }
}