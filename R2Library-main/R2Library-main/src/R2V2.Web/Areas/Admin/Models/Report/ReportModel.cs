#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using R2V2.Core.Admin;
using R2V2.Core.Institution;
using R2V2.Core.Publisher;
using R2V2.Core.Reports;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Core.Territory;
using R2V2.Web.Helpers;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class ReportModel : AdminBaseModel
    {
        private readonly List<ReportIpAddressRange> _ipAddressRanges = new List<ReportIpAddressRange>();

        private SelectList _periodList;
        private SelectList _sortByList;

        public ReportModel()
        {
        }

        public ReportModel(IAdminInstitution institution)
            : base(institution)
        {
        }

        public ReportModel(IAdminInstitution institution, ReportQuery reportQuery)
            : base(institution)
        {
            if (institution == null)
            {
                InstitutionId = 0;
                IsLicenseTypeEnabled = true;
            }
            else
            {
                InstitutionId = reportQuery.InstitutionId;
                if (InstitutionId == 0)
                {
                    IsLicenseTypeEnabled = true;
                }
                else
                {
                    IsLicenseTypeEnabled = institution.AccountStatus != null &&
                                           institution.AccountStatus.Id != AccountStatus.Trial &&
                                           institution.AccountStatus.Id != AccountStatus.TrialExpired;
                }
            }

            ReportQuery = reportQuery;
        }

        public string ResourceFilter { get; set; }
        public ReportQuery ReportQuery { get; set; }

        [Display(Name = @"Practice Area:")]
        public List<SelectListItem> PracticeAreaList { get; } = new List<SelectListItem>();

        [Display(Name = @"Discipline:")]
        public List<SelectListItem> SpecialtyList { get; } = new List<SelectListItem>();

        [Display(Name = @"Publisher:")] public List<SelectListItem> PublisherList { get; } = new List<SelectListItem>();

        [Display(Name = @"Title:")] public List<SelectListItem> ResourceList { get; } = new List<SelectListItem>();

        [Display(Name = @"Purchased")] public bool IncludePurchasedTitles => ReportQuery.IncludePurchasedTitles;

        [Display(Name = @"PDA")] public bool IncludePdaTitles => ReportQuery.IncludePdaTitles;

        [Display(Name = @"TOC")] public bool IncludeTocTitles => ReportQuery.IncludeTocTitles;

        [Display(Name = @"Trial")] public bool IncludeTrialStats => ReportQuery.IncludeTrialStats;

        [Display(Name = @"Institution Type:")]
        public List<SelectListItem> InstitutionTypeList { get; } = new List<SelectListItem>();

        [Display(Name = @"Territory:")] public List<SelectListItem> TerritoryList { get; } = new List<SelectListItem>();


        [Display(Name = @"Period:")]
        public SelectList PeriodList => _periodList ??
                                        (_periodList = new SelectList(new List<SelectListItem>
                                        {
                                            new SelectListItem
                                            {
                                                Text = @"last 12 months", Value = $"{ReportPeriod.LastTwelveMonths}"
                                            },
                                            new SelectListItem
                                                { Text = @"last 6 months", Value = $"{ReportPeriod.LastSixMonths}" },
                                            new SelectListItem
                                                { Text = @"last 30 days", Value = $"{ReportPeriod.Last30Days}" },
                                            new SelectListItem
                                                { Text = @"previous month", Value = $"{ReportPeriod.PreviousMonth}" },
                                            new SelectListItem
                                                { Text = @"current month", Value = $"{ReportPeriod.CurrentMonth}" },
                                            new SelectListItem
                                            {
                                                Text = @"specify a date range", Value = $"{ReportPeriod.UserSpecified}"
                                            },
                                            new SelectListItem
                                            {
                                                Text = $@"{DateTime.Now.Year} entire year",
                                                Value = $"{ReportPeriod.CurrentYear}"
                                            },
                                            new SelectListItem
                                            {
                                                Text = $@"{DateTime.Now.Year - 1} entire year",
                                                Value = $"{ReportPeriod.LastYear}"
                                            },
                                            new SelectListItem
                                                { Text = @"current quarter", Value = $"{ReportPeriod.CurrentQuarter}" },
                                            new SelectListItem
                                            {
                                                Text = @"previous quarter", Value = $"{ReportPeriod.PreviousQuarter}"
                                            }
                                        }, "Value", "Text", (int)ReportQuery.Period));

        //public ReportSortBy SortBy { get; set; }

        [Display(Name = @"Sort By:")]
        public SelectList SortByList => _sortByList ??
                                        (_sortByList = new SelectList(new List<SelectListItem>
                                        {
                                            new SelectListItem { Text = @"Title", Value = $"{ReportSortBy.Title}" },
                                            new SelectListItem
                                                { Text = @"License Count", Value = $"{ReportSortBy.LicenseCount}" },
                                            new SelectListItem
                                            {
                                                Text = @"Content Retrievals",
                                                Value = $"{ReportSortBy.ContentRetrievals}"
                                            },
                                            new SelectListItem
                                            {
                                                Text = @"Content Turnaways", Value = $"{ReportSortBy.ContentTurnaways}"
                                            },
                                            new SelectListItem
                                            {
                                                Text = @"Access Turnaways", Value = $"{ReportSortBy.AccessTurnaways}"
                                            },
                                            new SelectListItem
                                            {
                                                Text = @"First Purchase Date",
                                                Value = $"{ReportSortBy.FirstPurchaseDate}"
                                            },
                                            new SelectListItem
                                                { Text = @"Sessions", Value = $"{ReportSortBy.SessionCount}" },
                                            new SelectListItem
                                                { Text = @"PDA Views", Value = $"{ReportSortBy.PdaViews}" },
                                            new SelectListItem
                                                { Text = @"R2 Release Date", Value = $"{ReportSortBy.ReleaseDate}" },
                                            new SelectListItem
                                                { Text = @"Copyright Year", Value = $"{ReportSortBy.Copyright}" }
                                        }, "Value", "Text", (int)ReportQuery.SortBy));

        [Display(Name = @"Date Range: ")]
        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        [DateTenYears("DateRangeStart")]
        public DateTime? DateRangeStart => ReportQuery.DateRangeStart;

        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        [DateTenYears("DateRangeEnd")]
        public DateTime? DateRangeEnd => ReportQuery.DateRangeEnd;

        [Display(Name = @"IP Address Ranges:")]
        public bool FilterByIpRanges => ReportQuery.FilterByIpRanges;

        public IEnumerable<ReportIpAddressRange> IpAddressRanges => _ipAddressRanges;

        public bool IsSaveEnabled { get; set; }

        public bool IsLicenseTypeEnabled { get; set; }

        public string DebugInfo { get; set; }

        public ReportFrequency Frequency { get; set; }
        public int ReportId { get; set; }
        public string ActionType { get; set; }
        public ReportType Type { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        [Display(Name = @"Name:")] public string PublisherName { get; set; }
        [Display(Name = @"Vendor #:")] public string VendorNumber { get; set; }
        public IEnumerable<ReportResource> Items { get; set; }
        public int ResourceCount { get; set; }
        public int PageResourceFirstNumber { get; set; }
        public int PageResourceLastNumber { get; set; }

        public IEnumerable<PageLink> PageLinks { get; set; }

        public PageLink NextLink { get; set; }
        public PageLink PreviousLink { get; set; }

        public PageLink FirstLink { get; set; }
        public PageLink LastLink { get; set; }

        public bool DisableTocLicenseType { get; set; }

        public void AddIpAddressRanges(IEnumerable<Core.Authentication.IpAddressRange> ipAddressRanges)
        {
            foreach (var ipAddressRange in ipAddressRanges)
            {
                var ipRange = new ReportIpAddressRange
                {
                    Id = ipAddressRange.Id,
                    OctetA = ipAddressRange.OctetA,
                    OctetB = ipAddressRange.OctetB,
                    OctetCEnd = ipAddressRange.OctetCEnd,
                    OctetCStart = ipAddressRange.OctetCStart,
                    OctetDEnd = ipAddressRange.OctetDEnd,
                    OctetDStart = ipAddressRange.OctetDStart
                };
                _ipAddressRanges.Add(ipRange);
            }
        }

        public void InitializeLists(IEnumerable<IPublisher> publishers, int publisherId)
        {
            foreach (var publisher in publishers)
            {
                PublisherList.Add(new SelectListItem { Text = publisher.Name, Value = $"{publisher.Id}" });
            }

            var selectedPublisher = publishers.FirstOrDefault(x => x.Id == publisherId);
            if (selectedPublisher != null)
            {
                PublisherName = selectedPublisher.DisplayName ?? selectedPublisher.Name;
                VendorNumber = selectedPublisher.VendorNumber;
            }
        }

        public void InitializeLists(List<IPracticeArea> practiceAreas, List<ISpecialty> specialties,
            List<IResource> resources, IEnumerable<IPublisher> publishers, List<InstitutionType> institutionTypes)
        {
            List<IResource> institutionTitles;
            var allSelectListItem = new SelectListItem { Text = @"All", Value = "0" };
            var publisherList = publishers as IList<IPublisher> ?? publishers.ToList();
            if (Institution != null && Institution.Id > 0)
            {
                var institutionResources = Institution.Licenses.ToDictionary(x => x.ResourceId);
                institutionTitles = resources
                    .Where(resource => IncludeResource(resource, institutionResources.ContainsKey(resource.Id)))
                    .OrderBy(x => x.SortTitle).ToList();
            }
            else
            {
                var publisherToSearch = publisherList.Where(x =>
                    (x.ConsolidatedPublisher != null && x.ConsolidatedPublisher.Id == ReportQuery.PublisherId) ||
                    x.Id == ReportQuery.PublisherId).Select(x => x.Id);

                institutionTitles = ReportQuery.PublisherId > 0
                    ? resources.Where(resource =>
                            !resource.NotSaleable && publisherToSearch.Contains(resource.PublisherId))
                        .OrderBy(x => x.SortTitle).ToList()
                    : resources.Where(resource => !resource.NotSaleable).OrderBy(x => x.SortTitle).ToList();
            }

            ResourceList.Add(allSelectListItem);
            if (!institutionTitles.Any())
            {
                institutionTitles = resources.OrderBy(x => x.SortTitle).ToList();
            }

            PopulateResource(institutionTitles);

            // practice areas
            var institutionPracticeAreas = practiceAreas
                .Where(practiceArea => IsPracticeAreaInResourceList(practiceArea, institutionTitles)).ToList();
            PracticeAreaList.Add(allSelectListItem);
            if (!institutionPracticeAreas.Any())
            {
                institutionPracticeAreas = practiceAreas.ToList();
            }

            foreach (var institutionPracticeArea in institutionPracticeAreas)
            {
                PracticeAreaList.Add(new SelectListItem
                    { Text = institutionPracticeArea.Name, Value = $"{institutionPracticeArea.Id}" });
            }

            // specialty
            var institutionSpecialties =
                specialties.Where(x => IsSpecialyInResourceList(x, institutionTitles)).ToList();
            SpecialtyList.Add(allSelectListItem);
            if (!institutionSpecialties.Any())
            {
                institutionSpecialties = specialties.ToList();
            }

            foreach (var institutionSpecialty in institutionSpecialties)
            {
                SpecialtyList.Add(new SelectListItem
                    { Text = institutionSpecialty.Name, Value = $"{institutionSpecialty.Id}" });
            }

            // publishers
            var institutionPublishers = publisherList
                .Where(publisher => IsPublisherInResourceList(publisher, institutionTitles)).ToList();
            PublisherList.Add(allSelectListItem);
            if (!institutionPublishers.Any())
            {
                institutionPublishers = publisherList.ToList();
            }

            foreach (var publisher in institutionPublishers)
            {
                PublisherList.Add(new SelectListItem { Text = publisher.Name, Value = $"{publisher.Id}" });
            }

            //institution Types
            if (institutionTypes != null && institutionTypes.Any())
            {
                InstitutionTypeList.Add(allSelectListItem);
                foreach (var institutionType in institutionTypes)
                {
                    InstitutionTypeList.Add(new SelectListItem
                        { Text = institutionType.Name, Value = $"{institutionType.Id}" });
                }
            }
        }

        public void InitializeLists(List<IPracticeArea> practiceAreas, List<ISpecialty> specialties,
            List<IResource> resources, IEnumerable<IPublisher> publishers, List<InstitutionType> institutionTypes,
            List<ITerritory> territories)
        {
            InitializeLists(practiceAreas, specialties, resources, publishers, institutionTypes);
            if (territories != null && territories.Any())
            {
                TerritoryList.Add(new SelectListItem { Text = @"All", Value = "" });
                foreach (var territory in territories)
                {
                    TerritoryList.Add(new SelectListItem { Text = territory.Name, Value = $"{territory.Code}" });
                }
            }
        }

        public void PopulateResource(IEnumerable<IResource> resources)
        {
            var resource = resources.FirstOrDefault(x => x.Id == ReportQuery.ResourceId);
            if (resource != null)
            {
                ResourceFilter = $"{resource.Title} ({resource.Isbn}-Edition: {resource.Edition})";
            }
        }

        private bool IsPracticeAreaInResourceList(IPracticeArea practiceArea, IEnumerable<IResource> institutionTitles)
        {
            return institutionTitles.Any(resource => resource.PracticeAreas.Any(x => x.Id == practiceArea.Id));
        }

        private bool IsSpecialyInResourceList(ISpecialty specialty, IEnumerable<IResource> institutionTitles)
        {
            return institutionTitles.Any(resource => resource.Specialties.Any(x => x.Id == specialty.Id));
        }

        private bool IsPublisherInResourceList(IPublisher publisher, IEnumerable<IResource> institutionTitles)
        {
            return institutionTitles.Any(resource =>
                resource.Publisher.Id == publisher.Id || (resource.Publisher.ConsolidatedPublisher != null &&
                                                          resource.Publisher.ConsolidatedPublisher.Id == publisher.Id));
        }

        private bool IncludeResource(IResource resource, bool institutionOwnsResource)
        {
            if (institutionOwnsResource)
            {
                return true;
            }

            return !(IncludePurchasedTitles && !IncludeTocTitles && !IncludePdaTitles) && !resource.NotSaleable;
        }

        public string ToDebugString()
        {
            return new StringBuilder("Report = [")
                .Append($"InstitutionId: {InstitutionId}")
                .Append($", ReportType: {Type.ToDescription()}")
                .Append($", ReportId: {ReportId}")
                .Append($", Period: {ReportQuery.Period}")
                .Append($", DateRangeStart: {ReportQuery.DateRangeStart}")
                .Append($", DateRangeEnd: {ReportQuery.DateRangeEnd}").AppendLine().Append("\t")
                .Append($", PracticeAreaId: {ReportQuery.PracticeAreaId}")
                .Append($", SpecialtyId: {ReportQuery.SpecialtyId}")
                .Append($", PublisherId: {ReportQuery.PublisherId}")
                .Append($", ResourceId: {ReportQuery.ResourceId}")
                .Append($", IncludeTrialStats: {ReportQuery.IncludeTrialStats}").AppendLine().Append("\t")
                .Append($", IncludePdaTitles: {ReportQuery.IncludePdaTitles}").AppendLine().Append("\t")
                .Append($", IncludeTocTitles: {ReportQuery.IncludeTocTitles}").AppendLine().Append("\t")
                .Append($", IncludePurchasedTitles: {ReportQuery.IncludePurchasedTitles}").AppendLine().Append("\t")
                .Append($", FilterByIpRanges: {ReportQuery.FilterByIpRanges}")
                .Append(
                    $", EditableIpAddressRange: {(ReportQuery.EditableIpAddressRange != null ? ReportQuery.EditableIpAddressRange.ToDebugString() : "null")}")
                .AppendLine().Append("\t")
                .Append(
                    $", SelectedIpAddressRangeIds: {(ReportQuery.SelectedIpAddressRangeIds == null ? "null" : string.Join(",", ReportQuery.SelectedIpAddressRangeIds))}")
                .AppendLine().Append("\t")
                .Append($", Page: {ReportQuery.Page}")
                .Append($", Name: {Name}")
                .Append($", EmailAddress: {EmailAddress}")
                .Append($", Frequency: {Frequency}")
                .Append($", ActionType: {ActionType}")
                .Append("]").ToString();
        }

        public void OverRidePeriodList(SelectList selectList)
        {
            _periodList = selectList;
        }
    }
}