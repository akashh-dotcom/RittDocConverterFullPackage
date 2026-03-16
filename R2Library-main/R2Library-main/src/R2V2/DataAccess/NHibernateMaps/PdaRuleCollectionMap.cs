#region

using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class PdaRuleCollectionMap : BaseMap<PdaRuleCollection>
    {
        public PdaRuleCollectionMap()
        {
            Table("dbo.tPdaRuleCollection");
            Id(x => x.Id, "iPdaRuleCollectionId").GeneratedBy.Identity();
            Map(x => x.PdaRuleId, "iPdaRuleId");
            Map(x => x.CollectionId, "iCollectionId");
        }
    }
}