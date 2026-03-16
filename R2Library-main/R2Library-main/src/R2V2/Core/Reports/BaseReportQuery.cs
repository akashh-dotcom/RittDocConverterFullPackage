#region

using System;

#endregion

namespace R2V2.Core.Reports
{
    public class BaseReportQuery
    {
        public BaseReportQuery()
        {
        }

        public BaseReportQuery(SavedReport savedReport)
        {
            Period = savedReport.Period;
            PracticeAreaId = savedReport.PracticeAreaId;
            SpecialtyId = savedReport.SpecialtyId;
            PublisherId = savedReport.PublisherId;
            ResourceId = savedReport.ResourceId;
            InstitutionId = savedReport.InstitutionId;
            IncludePurchased = savedReport.IncludePurchased;
            IncludePda = savedReport.IncludePda;
            IncludeToc = savedReport.IncludeToc;
            IncludeTrialStats = savedReport.IncludeTrialStats;
            HasIpFilter = savedReport.HasIpFilter;
            PeriodStartDate = savedReport.PeriodStartDate;
            PeriodEndDate = savedReport.PeriodEndDate;
            ReportId = savedReport.Id;
            SortBy = ReportSortBy.Title;
            Type = (ReportType)savedReport.Type;
        }

        public ReportPeriod Period { get; set; }
        public int PracticeAreaId { get; set; }
        public int SpecialtyId { get; set; }
        public int PublisherId { get; set; }
        public int ResourceId { get; set; }
        public int InstitutionId { get; set; }
        public bool IncludePurchased { get; set; }
        public bool IncludePda { get; set; }
        public bool IncludeToc { get; set; }
        public bool IncludeTrialStats { get; set; }
        public bool HasIpFilter { get; set; }

        public DateTime? PeriodStartDate { get; set; }
        public DateTime? PeriodEndDate { get; set; }

        public int ReportId { get; set; }

        public ReportSortBy SortBy { get; set; }

        public int[] InstitutionIpRangeIds { get; set; }
        public string TerritoryCode { get; set; }

        public int InstitutionTypeId { get; set; }

        public ReportType Type { get; set; }
    }
}