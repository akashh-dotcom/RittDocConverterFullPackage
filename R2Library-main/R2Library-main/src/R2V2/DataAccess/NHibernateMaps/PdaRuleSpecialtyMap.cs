#region

using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class PdaRuleSpecialtyMap : BaseMap<PdaRuleSpecialty>
    {
        public PdaRuleSpecialtyMap()
        {
            Table("dbo.tPdaRuleSpecialty");
            Id(x => x.Id, "iPdaRuleSpecialtyId").GeneratedBy.Identity();
            Map(x => x.PdaRuleId, "iPdaRuleId");
            Map(x => x.SpecialtyId, "iSpecialtyId");
        }
    }
}