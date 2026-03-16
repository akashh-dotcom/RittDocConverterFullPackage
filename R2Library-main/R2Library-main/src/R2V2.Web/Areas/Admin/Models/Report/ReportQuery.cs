#region

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Routing;
using R2V2.Core.Reports;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    [Serializable]
    public class ReportQuery
    {
        private int _page;

        public ReportQuery()
        {
        }

        public ReportQuery(ReportQuery reportQuery)
        {
            InstitutionId = reportQuery.InstitutionId;
            Period = reportQuery.Period;
            DateRangeStart = reportQuery.DateRangeStart;
            DateRangeEnd = reportQuery.DateRangeEnd;

            FilterByIpRanges = reportQuery.FilterByIpRanges;
            EditableIpAddressRange = reportQuery.EditableIpAddressRange;
            SelectedIpAddressRangeIds = reportQuery.SelectedIpAddressRangeIds;

            PracticeAreaId = reportQuery.PracticeAreaId;
            SpecialtyId = reportQuery.SpecialtyId;
            PublisherId = reportQuery.PublisherId;
            ResourceId = reportQuery.ResourceId;

            IncludePurchasedTitles = reportQuery.IncludePurchasedTitles;
            IncludePdaTitles = reportQuery.IncludePdaTitles;
            IncludeTocTitles = reportQuery.IncludeTocTitles;
            IncludeTrialStats = reportQuery.IncludeTrialStats;

            Page = reportQuery.Page;
            InstitutionTypeId = reportQuery.InstitutionTypeId;
        }

        public ReportQuery(SavedReportDetail detail)
        {
            //ActionType = reportQuery.ActionType;
            DateRangeEnd = detail.ReportQuery.DateRangeEnd;
            DateRangeStart = detail.ReportQuery.DateRangeStart;
            Description = detail.Description;
            EditableIpAddressRange = detail.ReportQuery.EditableIpAddressRange;
            EmailAddress = detail.EmailAddress;
            FilterByIpRanges = detail.FilterByIpRanges;
            Frequency = detail.Frequency;
            InstitutionId = detail.InstitutionId;
            Name = detail.Name;
            Page = detail.ReportQuery.Page;
            Period = detail.ReportQuery.Period;
            PracticeAreaId = detail.ReportQuery.PracticeAreaId;
            SpecialtyId = detail.ReportQuery.SpecialtyId;
            PublisherId = detail.ReportQuery.PublisherId;
            ReportId = detail.ReportId;
            ReportTypeId = (int)detail.Type;
            ResourceId = detail.ReportQuery.ResourceId;
            SelectedIpAddressRangeIds = detail.ReportQuery.SelectedIpAddressRangeIds;

            IncludePurchasedTitles = detail.IncludePurchasedTitles;
            IncludePdaTitles = detail.IncludePdaTitles;
            IncludeTocTitles = detail.IncludeTocTitles;
            IncludeTrialStats = detail.IncludeTrialStats;
        }

        public int InstitutionId { get; set; }
        public ReportPeriod Period { get; set; }
        public DateTime? DateRangeStart { get; set; }
        public DateTime? DateRangeEnd { get; set; }

        public bool FilterByIpRanges { get; set; }
        public ReportIpAddressRange EditableIpAddressRange { get; set; }
        public List<int> SelectedIpAddressRangeIds { get; set; }

        public int PracticeAreaId { get; set; }
        public int SpecialtyId { get; set; }
        public int PublisherId { get; set; }
        public int ResourceId { get; set; }
        public bool IncludePurchasedTitles { get; set; }
        public bool IncludePdaTitles { get; set; }
        public bool IncludeTocTitles { get; set; }

        public bool IncludeTrialStats { get; set; }

        public int InstitutionTypeId { get; set; }
        public ResourceStatus ResourceStatus { get; set; }

        public int Page
        {
            get => _page == 0 ? 1 : _page;
            set => _page = value;
        }

        public int ReportId { get; set; }
        public int ReportTypeId { get; set; }

        public string TerritoryCode { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string Description { get; set; }
        public ReportFrequency Frequency { get; set; }

        public ReportSortBy SortBy { get; set; }

        public bool DefaultQuery => Period == ReportPeriod.Last30Days && Page == 1
                                                                      && !IncludePurchasedTitles && !IncludePdaTitles
                                                                      && !IncludeTocTitles && !IncludeTrialStats
                                                                      && ReportId == 0;

        public bool DefaultSalesQuery => Period == ReportPeriod.Last30Days && Page == 1
                                                                           && InstitutionId == 0 && ResourceId == 0
                                                                           && InstitutionTypeId == 0 &&
                                                                           string.IsNullOrWhiteSpace(TerritoryCode)
                                                                           && PublisherId == 0;

        public RouteValueDictionary ToExportValues()
        {
            var routeValueDictionary = new RouteValueDictionary(new { Area = "Admin" })
            {
                { "InstitutionId", InstitutionId },
                { "PublisherId", PublisherId },
                { "Period", Period },
                { "Page", 0 },
                { "IncludePurchasedTitles", IncludePurchasedTitles },
                { "IncludePdaTitles", IncludePdaTitles },
                { "IncludeTocTitles", IncludeTocTitles },
                { "DateRangeStart", DateRangeStart?.ToShortDateString() },
                { "DateRangeEnd", DateRangeEnd?.ToShortDateString() },
                { "FilterByIpRanges", FilterByIpRanges },
                { "PracticeAreaId", PracticeAreaId },
                { "SpecialtyId", SpecialtyId },
                { "ResourceId", ResourceId },
                { "ReportTypeId", ReportTypeId },
                { "IncludeTrialStats", IncludeTrialStats },
                { "InstitutionTypeId", InstitutionTypeId }
            };
            return routeValueDictionary;
        }

        public RouteValueDictionary ToRouteValues()
        {
            return ToRouteValues(Page);
        }

        public RouteValueDictionary ToAdminRouteValues(int institutionId)
        {
            InstitutionId = institutionId;
            return ToRouteValues(0);
        }

        public RouteValueDictionary ToRouteValues(int page)
        {
            // todo: not sure I like this here - sjs - 7/13/2012
            RouteValueDictionary routeValueDictionary;
            if (ReportId > 0)
            {
                routeValueDictionary = new RouteValueDictionary(new { Area = "Admin" })
                {
                    { "ReportId", ReportId }
                };
            }
            else
            {
                routeValueDictionary = new RouteValueDictionary(new { Area = "Admin" }) { { "Period", Period } };
                if (Period == ReportPeriod.UserSpecified)
                {
                    routeValueDictionary.Add("DateRangeStart", DateRangeStart?.ToString("d"));
                    routeValueDictionary.Add("DateRangeEnd", DateRangeEnd?.ToString("d"));
                }
            }

            routeValueDictionary.Add("InstitutionId", InstitutionId);

            if (page != 1)
            {
                routeValueDictionary.Add("Page", page);
            }

            if (PublisherId > 0)
            {
                routeValueDictionary.Add("PublisherId", PublisherId);
            }

            if (PracticeAreaId > 0)
            {
                routeValueDictionary.Add("PracticeAreaId", PracticeAreaId);
            }

            if (SpecialtyId > 0)
            {
                routeValueDictionary.Add("SpecialtyId", SpecialtyId);
            }

            if (IncludeTocTitles)
            {
                routeValueDictionary.Add("IncludeTocTitles", IncludeTocTitles);
            }

            if (IncludeTrialStats)
            {
                routeValueDictionary.Add("IncludeTrialStats", IncludeTrialStats);
            }

            if (IncludePdaTitles)
            {
                routeValueDictionary.Add("IncludePdaTitles", IncludePdaTitles);
            }

            if (IncludePurchasedTitles)
            {
                routeValueDictionary.Add("IncludePurchasedTitles", IncludePurchasedTitles);
            }

            if (InstitutionTypeId > 0)
            {
                routeValueDictionary.Add("InstitutionTypeId", InstitutionTypeId);
            }

            return routeValueDictionary;
        }

        public string ToDebugString()
        {
            return new StringBuilder("ReportQuery = [")
                .Append($"InstitutionId: {InstitutionId}")
                .Append($", ReportTypeId: {ReportTypeId}")
                .Append($", ReportId: {ReportId}")
                .Append($", Period: {Period}")
                .Append($", DateRangeStart: {DateRangeStart}")
                .Append($", DateRangeEnd: {DateRangeEnd}").AppendLine().Append("\t")
                .Append($", PracticeAreaId: {PracticeAreaId}")
                .Append($", SpecialtyId: {SpecialtyId}")
                .Append($", PublisherId: {PublisherId}")
                .Append($", ResourceId: {ResourceId}")
                .Append($", IncludePurchasedTitles: {IncludePurchasedTitles}").AppendLine().Append("\t")
                .Append($", IncludePdaTitles: {IncludePdaTitles}").AppendLine().Append("\t")
                .Append($", IncludeTocTitles: {IncludeTocTitles}").AppendLine().Append("\t")
                .Append($", FilterByIpRanges: {FilterByIpRanges}")
                .Append(
                    $", EditableIpAddressRange: {(EditableIpAddressRange != null ? EditableIpAddressRange.ToDebugString() : "null")}")
                .Append(
                    $", SelectedIpAddressRangeIds: {(SelectedIpAddressRangeIds == null ? "null" : string.Join(",", SelectedIpAddressRangeIds))}")
                .Append($", Page: {Page}")
                .Append($", Name: {Name}")
                .Append($", EmailAddress: {EmailAddress}")
                .Append($", Description: {Description}")
                .Append($", Frequency: {Frequency}")
                .Append($", IncludeTrialStats: {IncludeTrialStats}")
                .Append($", InstitutionTypeId: {InstitutionTypeId}")
                .Append("]").ToString();
        }

        public BaseReportQuery ToBaseReportQuery()
        {
            var baseQuery = new BaseReportQuery
            {
                Period = Period,
                PracticeAreaId = PracticeAreaId,
                SpecialtyId = SpecialtyId,
                PublisherId = PublisherId,
                ResourceId = ResourceId,
                InstitutionId = InstitutionId,
                IncludePurchased = IncludePurchasedTitles,
                IncludePda = IncludePdaTitles,
                IncludeToc = IncludeTocTitles,
                IncludeTrialStats = IncludeTrialStats,
                PeriodStartDate = DateRangeStart,
                PeriodEndDate = DateRangeEnd,
                ReportId = ReportId,
                HasIpFilter = FilterByIpRanges,
                SortBy = SortBy,
                TerritoryCode = TerritoryCode,
                InstitutionTypeId = InstitutionTypeId,
                Type = (ReportType)ReportTypeId
            };

            if (SelectedIpAddressRangeIds != null)
            {
                baseQuery.InstitutionIpRangeIds = SelectedIpAddressRangeIds.ToArray();
            }

            return baseQuery;
        }
    }
}