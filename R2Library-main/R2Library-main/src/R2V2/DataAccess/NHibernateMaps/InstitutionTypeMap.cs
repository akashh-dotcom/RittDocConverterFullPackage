#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class InstitutionTypeMap : BaseMap<InstitutionType>
    {
        public InstitutionTypeMap()
        {
            Table("tInstitutionType");
            Id(x => x.Id).Column("iInstitutionTypeId").GeneratedBy.Identity();
            Map(x => x.Code).Column("vchInstitutionTypeCode");
            Map(x => x.Name).Column("vchInstitutionTypeName");
        }
    }
}