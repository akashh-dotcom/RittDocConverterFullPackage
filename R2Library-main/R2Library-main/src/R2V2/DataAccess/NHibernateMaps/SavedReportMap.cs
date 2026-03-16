#region

using R2V2.Core.Reports;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class SavedReportMap : BaseMap<SavedReport>
    {
        public SavedReportMap()
        {
            Table("tSavedReports");

            Id(x => x.Id).Column("iReportId").GeneratedBy.Identity();

            Map(x => x.UserId, "iUserId");
            Map(x => x.Name, "vchReportName");
            Map(x => x.Type, "iReportType");
            Map(x => x.Frequency, "iFrequency");
            Map(x => x.CreationDate, "dtCreationDate");
            Map(x => x.LastUpdate, "dtLastUpdate");
            Map(x => x.Email, "vchEmail");
            Map(x => x.InstitutionId, "iInstitutionId");
            Map(x => x.PublisherId, "iPublisherId");
            Map(x => x.ResourceId, "iResourceId");
            Map(x => x.HasIpFilter, "bIpFilter");
            Map(x => x.Description, "vchDescription");
            Map(x => x.PracticeAreaId, "iLibraryId");
            Map(x => x.SpecialtyId, "iSpecialtyId");

            Map(x => x.IncludePurchased, "bIncludePurchased");
            Map(x => x.IncludePda, "bIncludePda");
            Map(x => x.IncludeToc, "bIncludeToc");
            Map(x => x.IncludeTrialStats, "bIncludeTrialStats");

            Map(x => x.Period, "iPeriod").CustomType(typeof(ReportPeriod));
            Map(x => x.PeriodStartDate, "dtPeriodStartDate");
            Map(x => x.PeriodEndDate, "dtPeriodEndDate");

            HasMany(x => x.IpFilters).KeyColumn("iReportId").AsBag().Inverse().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();

            References(x => x.Institution).Column("iInstitutionId").ReadOnly();
        }
    }
}