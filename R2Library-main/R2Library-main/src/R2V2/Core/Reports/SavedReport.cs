#region

using System;
using System.Collections.Generic;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Reports
{
    public class SavedReport : ISoftDeletable
    {
        //public virtual IList<SavedReportIpFilter> IpFilters { get; set; }
        private readonly IList<SavedReportIpFilter> _ipFilters = new List<SavedReportIpFilter>();
        public virtual int Id { get; set; }
        public virtual int UserId { get; set; }

        public virtual string Name { get; set; }

        //public virtual string Type { get; set; }
        public virtual int Frequency { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual DateTime? LastUpdate { get; set; }
        public virtual string Email { get; set; }

        public virtual int InstitutionId { get; set; }
        public virtual int PublisherId { get; set; }


        public virtual int ResourceId { get; set; }
        public virtual int Type { get; set; }

        public virtual Institution.Institution Institution { get; set; }

        public virtual bool HasIpFilter { get; set; }
        public virtual string Description { get; set; }

        public virtual int PracticeAreaId { get; set; }
        public virtual int SpecialtyId { get; set; }
        public virtual IEnumerable<SavedReportIpFilter> IpFilters => _ipFilters;

        public virtual bool IncludePurchased { get; set; }
        public virtual bool IncludePda { get; set; }
        public virtual bool IncludeToc { get; set; }
        public virtual bool IncludeTrialStats { get; set; }
        public virtual ReportPeriod Period { get; set; }

        public virtual DateTime? PeriodStartDate { get; set; }
        public virtual DateTime? PeriodEndDate { get; set; }

        public virtual bool RecordStatus { get; set; }

        public virtual void AddIpFilter(SavedReportIpFilter ipFilter)
        {
            if (ipFilter != null)
            {
                _ipFilters.Add(ipFilter);
                ipFilter.Report = this;
            }
        }

        public virtual void AddIpFilter(string ipRangeStart, string ipRangeEnd)
        {
            var ipFilter = new SavedReportIpFilter
            {
                IpEndRange = ipRangeStart,
                IpStartRange = ipRangeEnd,
                RecordStatus = true
            };
            AddIpFilter(ipFilter);
        }


        public virtual void DeleteItem(SavedReportIpFilter ipFilter)
        {
            _ipFilters.Remove(ipFilter);
        }

        public virtual string ToDebugString()
        {
            return new StringBuilder("SavedReport = [")
                .Append($"Id = {Id}")
                .Append($", UserId = {UserId}")
                .Append($", Name = {Name}")
                .Append($", Type = {Type}")
                .Append($", Frequency = {Frequency}")
                .Append($", CreationDate = {CreationDate}")
                .Append($", LastUpdate = {LastUpdate}")
                .Append($", Email = {Email}")
                .Append($", RecordStatus = {RecordStatus}")
                .Append($", InstitutionId = {InstitutionId}")
                .Append($", PublisherId = {PublisherId}")
                .Append($", ResourceId = {ResourceId}")
                .Append($", HasIpFilter = {HasIpFilter}")
                .Append($", Description = {Description}")
                .Append($", PracticeAreaId = {PracticeAreaId}")
                .Append($", SpecialtyId = {SpecialtyId}")
                .Append($", IncludeTrialStats = {IncludeTrialStats}")
                .Append($", IncludeToc = {IncludeToc}")
                .Append($", IncludePda = {IncludePda}")
                .Append($", IncludePurchased = {IncludePurchased}")
                .Append("]").ToString();
        }
    }
}