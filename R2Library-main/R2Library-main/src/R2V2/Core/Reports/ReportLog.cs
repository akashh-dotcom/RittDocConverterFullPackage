#region

using System;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Reports
{
    public class ReportLog : AuditableEntity, ISoftDeletable
    {
        public virtual ReportType Type { get; set; }
        public virtual int? InstitutionId { get; set; }
        public virtual ReportPeriod Period { get; set; }
        public virtual DateTime DateRangeStart { get; set; }
        public virtual DateTime DateRangeEnd { get; set; }
        public virtual string IpFilter { get; set; }
        public virtual int? PracticeAreaId { get; set; }
        public virtual int? SpecialtyId { get; set; }

        public virtual int? PublisherId { get; set; }
        public virtual int? ResourceId { get; set; }
        public virtual bool IncludePurchasedTitles { get; set; }
        public virtual bool IncludePdaTitles { get; set; }
        public virtual bool IncludeTocTitles { get; set; }
        public virtual bool IncludeTrialStats { get; set; }
        public virtual int? InstitutionTypeId { get; set; }
        public virtual string TerritoryCode { get; set; }
        public virtual bool IsDefaultQuery { get; set; }
        public virtual ReportSortBy SortBy { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}