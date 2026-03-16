#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class InstitutionBrandingMap : BaseMap<InstitutionBranding>
    {
        public InstitutionBrandingMap()
        {
            Table("dbo.tInstitutionBranding");

            Id(x => x.Id, "iInstitutionBrandingId").GeneratedBy.Identity();
            Map(x => x.Message, "vchMessage");
            Map(x => x.InstitutionDisplayName, "vchInstitutionDisplayName");
            Map(x => x.LogoFileName, "vchLogoFileName");

            References(x => x.Institution).Column("iInstitutionId");
        }
    }
}