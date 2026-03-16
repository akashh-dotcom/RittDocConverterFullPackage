#region

using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class MedicalTermSynonymMap : BaseMap<MedicalTermSynonym>
    {
        public MedicalTermSynonymMap()
        {
            Table("tMedicalTermSynonyms");

            Id(x => x.Id, "iMedicalTermSynonymId").GeneratedBy.Identity();
            Map(x => x.Name, "vchSynonymTermName");

            References(x => x.MedicalTerm).Column("iMedicalTermsId");
        }
    }
}