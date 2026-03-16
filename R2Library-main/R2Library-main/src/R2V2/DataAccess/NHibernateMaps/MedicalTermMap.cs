#region

using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class MedicalTermMap : BaseMap<MedicalTerm>
    {
        public MedicalTermMap()
        {
            Table("tMedicalTerms");

            Id(x => x.Id, "iMedicalTermsId").GeneratedBy.Identity();
            Map(x => x.Name, "vchMedicalTermName");
        }
    }
}