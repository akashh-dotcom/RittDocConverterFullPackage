#region

using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class PdaRulePracticeAreaMap : BaseMap<PdaRulePracticeArea>
    {
        public PdaRulePracticeAreaMap()
        {
            Table("dbo.tPdaRulePracticeArea");
            Id(x => x.Id, "iPdaRulePracticeAreaId").GeneratedBy.Identity();
            Map(x => x.PdaRuleId, "iPdaRuleId");
            Map(x => x.PracticeAreaId, "iPracticeAreaId");
        }
    }
}