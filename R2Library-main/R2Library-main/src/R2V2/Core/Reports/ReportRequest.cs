#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Authentication;

#endregion

namespace R2V2.Core.Reports
{
    public class ReportRequest
    {
        private readonly List<IpAddressRange> _condensedIpAddressRanges = new List<IpAddressRange>();
        private readonly List<IpAddressRange> _ipAddressRanges = new List<IpAddressRange>();

        public int InstitutionId { get; set; }
        public DateTime DateRangeStart { get; set; }
        public DateTime DateRangeEnd { get; set; }

        public bool IncludePurchasedTitlesOnly { get; set; }
        public bool IncludePurchasedTitles { get; set; }
        public bool IncludePdaTitles { get; set; }
        public bool IncludeTocTitles { get; set; }
        public bool IncludeTrialStats { get; set; }

        public int PracticeAreaId { get; set; }
        public int SpecialtyId { get; set; }
        public int PublisherId { get; set; }
        public int ResourceId { get; set; }

        public bool IsPublisherUser { get; set; }
        public bool IsTrialAccount { get; set; }

        public int InstitutionTypeId { get; set; }

        public string TerritoryCode { get; set; }

        public ReportSortBy SortBy { get; set; }

        public ReportPeriod Period { get; set; }

        public ReportType Type { get; set; }

        public int Status { get; set; }

        public bool IsDefaultRequest()
        {
            bool isDefault;
            switch (Type)
            {
                case ReportType.AnnualFeeReport:
                    isDefault = Period == ReportPeriod.Last30Days;
                    break;
                case ReportType.CounterDeniedRequests:
                    isDefault = Period == ReportPeriod.LastTwelveMonths;
                    isDefault = isDefault && IncludePurchasedTitles;
                    isDefault = isDefault && IncludePdaTitles;
                    break;
                case ReportType.CounterBookRequests:
                case ReportType.CounterPlatformRequests:
                case ReportType.CounterSearchRequests:
                case ReportType.CounterSectionRequests:
                    isDefault = Period == ReportPeriod.LastTwelveMonths;
                    isDefault = isDefault && IncludePurchasedTitles;
                    isDefault = isDefault && IncludePdaTitles;
                    isDefault = isDefault && IncludeTrialStats;

                    break;
                case ReportType.PdaCountsReport:
                case ReportType.PublisherUser:
                    isDefault = Period == 0;
                    break;
                case ReportType.ApplicationUsageReport:
                    if (InstitutionId > 0)
                    {
                        isDefault = Period == ReportPeriod.LastTwelveMonths;
                    }
                    else
                    {
                        isDefault = Period == ReportPeriod.Last30Days;
                    }

                    break;
                case ReportType.ResourceUsageReport:

                    if (InstitutionId > 0)
                    {
                        isDefault = Period == ReportPeriod.LastTwelveMonths;
                        isDefault = isDefault && IncludePdaTitles;
                        isDefault = isDefault && IncludePurchasedTitles;
                    }
                    else
                    {
                        isDefault = Period == ReportPeriod.Last30Days;
                        isDefault = isDefault && !IncludePdaTitles;
                        isDefault = isDefault && !IncludePurchasedTitles;
                        isDefault = isDefault && InstitutionTypeId == 0;
                    }

                    isDefault = isDefault && !IncludeTocTitles;
                    isDefault = isDefault && !IncludeTrialStats;
                    isDefault = isDefault && PracticeAreaId == 0;
                    isDefault = isDefault && SpecialtyId == 0;
                    isDefault = isDefault && PublisherId == 0;
                    isDefault = isDefault && ResourceId == 0;
                    isDefault = isDefault && SortBy == ReportSortBy.Title;
                    isDefault = isDefault && !_ipAddressRanges.Any();
                    break;
                default:
                    isDefault = false;
                    break;
            }

            return isDefault;
        }

        public void AddIpAddressRange(IpAddressRange ipAddressRange)
        {
            _ipAddressRanges.Add(ipAddressRange);
        }

        public void AddIpAddressRanges(IEnumerable<IpAddressRange> ipAddressRanges)
        {
            _ipAddressRanges.AddRange(ipAddressRanges);
        }

        public IEnumerable<IpAddressRange> GetCondensedRanges()
        {
            if (!_condensedIpAddressRanges.Any() && _ipAddressRanges.Any())
            {
                var orderedIpRanges = _ipAddressRanges.OrderBy(x => x.IpNumberStart);
                IpAddressRange startIpAddressRange = null;
                foreach (var ipRange in orderedIpRanges)
                {
                    if (startIpAddressRange == null)
                    {
                        startIpAddressRange = ipRange;
                        continue;
                    }

                    var difference = ipRange.IpNumberStart - startIpAddressRange.IpNumberEnd;

                    if (difference <= 3)
                    {
                        if (difference == 3 && ipRange.OctetDStart == 1 && startIpAddressRange.OctetDEnd == 254)
                        {
                            startIpAddressRange.IpNumberEnd = ipRange.IpNumberEnd;
                            startIpAddressRange.OctetCEnd = ipRange.OctetCEnd;
                            startIpAddressRange.OctetDEnd = ipRange.OctetDEnd;
                            continue;
                        }

                        if (difference == 2 &&
                            ((ipRange.OctetDStart == 1 && startIpAddressRange.OctetDEnd == 255) ||
                             (ipRange.OctetDStart == 0 && startIpAddressRange.OctetDEnd == 254)))
                        {
                            startIpAddressRange.IpNumberEnd = ipRange.IpNumberEnd;
                            startIpAddressRange.OctetCEnd = ipRange.OctetCEnd;
                            startIpAddressRange.OctetDEnd = ipRange.OctetDEnd;
                            continue;
                        }

                        if (difference == 1 && ipRange.OctetDStart == 0 && startIpAddressRange.OctetDEnd == 255)
                        {
                            startIpAddressRange.IpNumberEnd = ipRange.IpNumberEnd;
                            startIpAddressRange.OctetCEnd = ipRange.OctetCEnd;
                            startIpAddressRange.OctetDEnd = ipRange.OctetDEnd;
                            continue;
                        }

                        _condensedIpAddressRanges.Add(startIpAddressRange);
                    }

                    _condensedIpAddressRanges.Add(startIpAddressRange);
                    startIpAddressRange = ipRange;
                }

                _condensedIpAddressRanges.Add(startIpAddressRange);
            }

            return _condensedIpAddressRanges;
        }
    }
}