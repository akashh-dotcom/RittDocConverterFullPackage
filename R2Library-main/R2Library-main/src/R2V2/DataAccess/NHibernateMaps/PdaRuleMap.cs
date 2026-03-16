#region

using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class PdaRuleMap : BaseMap<PdaRule>
    {
        public PdaRuleMap()
        {
            Table("dbo.tPdaRule");
            Id(x => x.Id, "iPdaRuleId").GeneratedBy.Identity();

            Map(x => x.Name, "vchRuleName");
            Map(x => x.MaxPrice, "decMaxPrice");
            Map(x => x.ExecuteForFuture, "tiFuture");
            Map(x => x.IncludeNewEditionFirm, "tiNewEditionFirm");
            Map(x => x.IncludeNewEditionPda, "tiNewEditionPda");
            Map(x => x.InstitutionId, "iInstitutionId");

            HasMany(x => x.PracticeAreas).KeyColumn("iPdaRuleId").AsBag().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
            HasMany(x => x.Specialties).KeyColumn("iPdaRuleId").AsBag().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
            HasMany(x => x.Collections).KeyColumn("iPdaRuleId").AsBag().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
        }
    }
}