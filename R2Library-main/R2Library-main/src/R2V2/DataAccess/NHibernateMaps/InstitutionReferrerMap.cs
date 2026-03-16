#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class InstitutionReferrerMap : BaseMap<InstitutionReferrer>
    {
        public InstitutionReferrerMap()
        {
            Table("dbo.tValidReferer");
            Id(x => x.Id, "iValidRefererID");
            Map(x => x.ValidReferer, "vchValidReferer");
            Map(x => x.InstitutionId, "iInstitutionId");
            References<Institution>(x => x.Institution).Column("iInstitutionId").ReadOnly();
        }
    }
}