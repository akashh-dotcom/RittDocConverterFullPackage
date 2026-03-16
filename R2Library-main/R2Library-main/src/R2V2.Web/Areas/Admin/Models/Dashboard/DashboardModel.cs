#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using R2V2.Core.Admin;
using R2V2.Core.Reports;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Dashboard
{
    public class DashboardModel : AdminBaseModel
    {
        private SelectList _periodList;

        public DashboardModel()
        {
            Period = ReportPeriod.Today;
        }

        public DashboardModel(AdminInstitution institution, InstitutionDashboardStatistics stats,
            List<InstitutionResource> institutionResources, List<string> notes, bool isResourceList)
            : base(institution)
        {
            if (stats != null)
            {
                QuickNotes = notes;
                DateRangeStart = stats.StartDate;
                DateRangeEnd = stats.EndDate;
                if (institutionResources != null && institutionResources.Any())
                {
                    if (!isResourceList)
                    {
                        PopulateHighlights(stats, institutionResources);
                        if (stats.Highlights != null)
                        {
                            MostPopularSpecialtyName = stats.Highlights.MostPopularSpecialtyName;
                            MostPopularSpecialtyCount = stats.Highlights.MostPopularSpecialtyCount;
                            LeastPopularSpecialtyName = stats.Highlights.LeastPopularSpecialtyName;
                            LeastPopularSpecialtyCount = stats.Highlights.LeastPopularSpecialtyCount;
                            TotalResourceCount = stats.Highlights.TotalResourceCount;
                        }

                        if (stats.AccountUsage != null)
                        {
                            ContentCount = stats.AccountUsage.ContentCount;
                            TocCount = stats.AccountUsage.TocCount;
                            SessionCount = stats.AccountUsage.SessionCount;
                            PrintCount = stats.AccountUsage.PrintCount;
                            EmailCount = stats.AccountUsage.EmailCount;
                            TurnawayConcurrencyCount = stats.AccountUsage.TurnawayConcurrencyCount;
                            TurnawayAccessCount = stats.AccountUsage.TurnawayAccessCount;
                        }
                    }
                    else
                    {
                        if (stats.Highlights != null)
                        {
                            TotalResourceCount = stats.Highlights.TotalResourceCount;
                        }

                        PopulateResourceLists(stats, institutionResources);
                    }
                }

                ShowNoPdaCollectionText = !institution.IsPdaEulaSigned;
                ShowNoEbookCollectionText = !institution.IsEulaSigned;
            }
        }

        [Display(Name = @"Start Date")] public DateTime DateRangeStart { get; set; }

        [Display(Name = @"End Date")] public DateTime DateRangeEnd { get; set; }

        public DashboardResource MostAccessed { get; set; }
        public DashboardResource LeastAccessed { get; set; }
        public DashboardResource MostTurnawayAccess { get; set; }
        public DashboardResource MostTurnawayConcurrent { get; set; }

        public List<DashboardResource> PurchasedList { get; set; }
        public List<DashboardResource> ArchivedList { get; set; }
        public List<DashboardResource> NewEditionPurchasedList { get; set; }
        public List<DashboardResource> PdaAddedList { get; set; }
        public List<DashboardResource> PdaAddedToCartList { get; set; }
        public List<DashboardResource> PdaNewEditionList { get; set; }

        public string MostPopularSpecialtyName { get; set; }
        public int MostPopularSpecialtyCount { get; set; }
        public int MostPopularSpecialtyId { get; set; }
        public string LeastPopularSpecialtyName { get; set; }
        public int LeastPopularSpecialtyCount { get; set; }
        public int LeastPopularSpecialtyId { get; set; }

        [Display(Name = @"Total Resources:  ")]
        public int TotalResourceCount { get; set; }

        [Display(Name = @"Successful Content Retrievals:")]
        public int ContentCount { get; set; }

        [Display(Name = @"TOC Retrievals:")] public int TocCount { get; set; }

        [Display(Name = @"User Sessions: ")] public int SessionCount { get; set; }

        [Display(Name = @"Print Requests: ")] public int PrintCount { get; set; }

        [Display(Name = @"Email Requests: ")] public int EmailCount { get; set; }

        [Display(Name = @"Concurrent Turnaways: ")]
        public int TurnawayConcurrencyCount { get; set; }

        [Display(Name = @"Access Turnaways:  ")]
        public int TurnawayAccessCount { get; set; }


        public List<string> QuickNotes { get; set; }

        public List<DashboardResource> FeaturedTitles { get; set; }

        public List<DashboardResource> SpecialTitles { get; set; }

        public List<DashboardResource> RecommendedTitles { get; set; }
        public ReportPeriod Period { get; set; }

        [Display(Name = @"Period:")]
        public SelectList PeriodList =>
            _periodList ??
            (_periodList = new SelectList(new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = GetDateSelectorString(ReportPeriod.PreviousMonth),
                    Value = $"{ReportPeriod.PreviousMonth}"
                },
                new SelectListItem
                {
                    //Text = "last 12 months",
                    Text = GetDateSelectorString(ReportPeriod.LastTwelveMonths),
                    Value = $"{ReportPeriod.LastTwelveMonths}"
                },
                new SelectListItem
                {
                    Text = GetDateSelectorString(ReportPeriod.LastSixMonths),
                    Value = $"{ReportPeriod.LastSixMonths}"
                },
                new SelectListItem
                {
                    Text = GetDateSelectorString(ReportPeriod.CurrentYear),
                    Value = $"{ReportPeriod.CurrentYear}"
                },
                new SelectListItem
                {
                    Text = GetDateSelectorString(ReportPeriod.LastYear),
                    Value = $"{ReportPeriod.LastYear}"
                },
                new SelectListItem
                {
                    Text = @"specify a date range",
                    Value = $"{ReportPeriod.UserSpecified}"
                }
            }, "Value", "Text", (int)Period));

        public PageLink HighLightLink { get; set; }
        public PageLink EbookCollectionLink { get; set; }
        public PageLink PdaCollectionLink { get; set; }

        public bool ShowNoPdaCollectionText { get; set; }
        public bool ShowNoEbookCollectionText { get; set; }
        public bool ShowEbookText { get; set; }

        public string DateRangeMax()
        {
            return $"{DateTime.Now.AddMonths(-1).Month:00}/{DateTime.Now.Year:0000}";
        }

        public string DateRangeMin()
        {
            return "01/2009";
        }

        public bool DisplayHightlights()
        {
            return MostAccessed != null ||
                   LeastAccessed != null ||
                   MostTurnawayConcurrent != null ||
                   MostTurnawayAccess != null ||
                   !string.IsNullOrWhiteSpace(MostPopularSpecialtyName) ||
                   !string.IsNullOrWhiteSpace(LeastPopularSpecialtyName);
        }

        public bool ShowAccountUsage()
        {
            return TotalResourceCount > 0 ||
                   ContentCount > 0 ||
                   TocCount > 0 ||
                   SessionCount > 0 ||
                   PrintCount > 0 ||
                   EmailCount > 0 ||
                   TurnawayConcurrencyCount > 0 ||
                   TurnawayAccessCount > 0;
        }

        public bool ShowOpen()
        {
            return Period == ReportPeriod.UserSpecified;
        }

        public string GetDateSelectorString(ReportPeriod period)
        {
            var startDate = DateTime.MinValue;
            var endDate = DateTime.MinValue;
            string dateText = null;
            switch (period)
            {
                case ReportPeriod.LastTwelveMonths:
                    SetStartEndDate(ReportPeriod.LastTwelveMonths, out startDate, out endDate);
                    break;
                case ReportPeriod.LastSixMonths:
                    SetStartEndDate(ReportPeriod.LastSixMonths, out startDate, out endDate);
                    break;
                case ReportPeriod.UserSpecified:
                    SetStartEndDate(ReportPeriod.UserSpecified, out startDate, out endDate);
                    break;
                case ReportPeriod.CurrentYear:
                    SetStartEndDate(ReportPeriod.CurrentYear, out startDate, out endDate);
                    break;
                case ReportPeriod.LastYear:
                    dateText = $"{DateTime.Now.AddYears(-1).Year} entire year";
                    break;
                default:
                    //case ReportPeriod.PreviousMonth:
                    SetStartEndDate(ReportPeriod.PreviousMonth, out startDate, out endDate, true);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(dateText))
            {
                return dateText;
            }

            if (endDate != DateTime.MinValue)
            {
                return $"{startDate.ToString("MMMM")} {startDate.Year} to {endDate.ToString("MMMM")} {endDate.Year}";
            }

            return $"{startDate.ToString("MMMM")} {startDate.Year}";
        }

        public void SetStartEndDate(ReportPeriod period, out DateTime startDate, out DateTime endDate,
            bool excludeEndDate = false)
        {
            var date = DateTime.Now.AddMonths(-1).Date;
            switch (period)
            {
                case ReportPeriod.LastTwelveMonths:
                    startDate = DateTime.Parse($"{date.AddMonths(-11).Month}/{1}/{date.AddMonths(-11).Year}");
                    endDate = date;
                    break;
                case ReportPeriod.LastSixMonths:
                    startDate = DateTime.Parse($"{date.AddMonths(-5).Month}/{1}/{date.AddMonths(-5).Year}");
                    endDate = date;
                    break;
                case ReportPeriod.UserSpecified:

                    if (DateRangeStart == DateTime.MinValue && DateRangeEnd == DateTime.MinValue)
                    {
                        startDate = DateTime.Parse($"{date.AddMonths(-12).Month}/{1}/{date.AddMonths(-12).Year}");
                        endDate = DateTime.Parse($"{date.AddMonths(-1).Month}/{1}/{date.AddMonths(-1).Year}");
                    }
                    else
                    {
                        startDate = DateTime.Parse($"{DateRangeStart.Month}/{1}/{DateRangeStart.Year}");
                        endDate = DateTime.Parse(
                            $"{DateRangeEnd.Month}/{DateTime.DaysInMonth(DateRangeEnd.Year, DateRangeEnd.Month)}/{DateRangeEnd.Year}");
                    }

                    break;
                case ReportPeriod.CurrentYear:
                    startDate = DateTime.Parse($"{1}/{1}/{date.Year}");
                    endDate = date;
                    break;
                case ReportPeriod.LastYear:
                    startDate = DateTime.Parse($"{1}/{1}/{date.AddYears(-1).Year}");
                    endDate = DateTime.Parse($"{12}/{31}/{date.AddYears(-1).Year}");
                    break;
                default:
                    //case ReportPeriod.PreviousMonth:
                    startDate = DateTime.Parse($"{date.Month}/{1}/{date.Year}");
                    endDate = excludeEndDate ? DateTime.MinValue : startDate.AddMonths(1).AddDays(-1);
                    break;
            }

            //Set to last min of the day in end date
            if (!excludeEndDate)
            {
                endDate = endDate.Date.AddDays(+1).AddMinutes(-1).AddSeconds(59);
            }
        }

        public bool EbookShowNoResourceText()
        {
            return (PurchasedList == null || !PurchasedList.Any()) &&
                   (ArchivedList == null || !ArchivedList.Any()) &&
                   (NewEditionPurchasedList == null || !NewEditionPurchasedList.Any());
        }

        public bool PdaShowNoResourceText()
        {
            return (PdaAddedList == null || !PdaAddedList.Any()) &&
                   (PdaAddedToCartList == null || !PdaAddedToCartList.Any()) &&
                   (PdaNewEditionList == null || !PdaNewEditionList.Any());
        }

        public void SetPageLinks()
        {
            var queryStringBuilder = new StringBuilder();

            queryStringBuilder.AppendFormat("?Period={0}", Period);

            if (DateRangeStart != DateTime.MinValue)
            {
                queryStringBuilder.AppendFormat("&DateRangeStart={0}", DateRangeStart.ToString("d"));
            }

            if (DateRangeEnd != DateTime.MinValue)
            {
                queryStringBuilder.AppendFormat("&DateRangeEnd={0}", DateRangeEnd.ToString("d"));
            }

            HighLightLink = new PageLink
            {
                Href = $"/Admin/Dashboard/Index/{InstitutionId}{queryStringBuilder}",
                Text = "Highlights",
                Active = true
            };
            EbookCollectionLink = new PageLink
            {
                Href = $"/Admin/Dashboard/EbookCollection/{InstitutionId}{queryStringBuilder}",
                Text = "Ebook Collection",
                Active = true
            };
            PdaCollectionLink = new PageLink
            {
                Href = $"/Admin/Dashboard/PdaCollection/{InstitutionId}{queryStringBuilder}",
                Text = "PDA Collection",
                Active = true
            };
        }


        private DashboardResource GetDashboardResource(int id, int stat, InstitutionResource ir)
        {
            if (id > 0)
            {
                var item = new DashboardResource(stat, ir, InstitutionId, DateRangeStart, DateRangeEnd);
                return item.InstitutionResource == null ? null : item;
            }

            return null;
            ;
        }

        private void PopulateHighlights(InstitutionStatistics stats, List<InstitutionResource> institutionResources)
        {
            if (stats.Highlights != null)
            {
                var mostAccessId = stats.Highlights.MostAccessedResourceId;
                var leastAccessId = stats.Highlights.LeastAccessedResourceId;
                var mostTurnawayAccessId = stats.Highlights.MostTurnawayAccessResourceId;
                var mostTurnawayConcurrentId = stats.Highlights.MostTurnawayConcurrentResourceId;
                DashboardResource item = null;
                if (mostAccessId > 0)
                {
                    item = new DashboardResource(stats.Highlights.MostAccessedCount,
                        institutionResources.FirstOrDefault(x => x.Id == mostAccessId), InstitutionId, DateRangeStart,
                        DateRangeEnd);
                    if (item.InstitutionResource == null)
                    {
                        item = null;
                    }
                }

                MostAccessed = GetDashboardResource(mostAccessId, stats.Highlights.MostAccessedCount,
                    institutionResources.FirstOrDefault(x => x.Id == mostAccessId));
                LeastAccessed = GetDashboardResource(leastAccessId, stats.Highlights.LeastAccessedCount,
                    institutionResources.FirstOrDefault(x => x.Id == leastAccessId));
                MostTurnawayAccess = GetDashboardResource(mostTurnawayAccessId,
                    stats.Highlights.MostTurnawayAccessCount,
                    institutionResources.FirstOrDefault(x => x.Id == mostTurnawayAccessId));
                MostTurnawayConcurrent = GetDashboardResource(mostTurnawayConcurrentId,
                    stats.Highlights.MostTurnawayConcurrentCount,
                    institutionResources.FirstOrDefault(x => x.Id == mostTurnawayConcurrentId));


                //MostAccessed = mostAccessId > 0 ? new DashboardResource(stats.Highlights.MostAccessedCount, institutionResources.FirstOrDefault(x => x.Id == mostAccessId), InstitutionId, DateRangeStart, DateRangeEnd) : null;
                //LeastAccessed = leastAccessId > 0 ? new DashboardResource(stats.Highlights.LeastAccessedCount, institutionResources.FirstOrDefault(x => x.Id == leastAccessId), InstitutionId, DateRangeStart, DateRangeEnd) : null;
                //MostTurnawayAccess = mostTurnawayAccessId > 0 ? new DashboardResource(stats.Highlights.MostTurnawayAccessCount, institutionResources.FirstOrDefault(x => x.Id == mostTurnawayAccessId), InstitutionId, DateRangeStart, DateRangeEnd) : null;
                //MostTurnawayConcurrent = mostTurnawayConcurrentId > 0 ? new DashboardResource(stats.Highlights.MostTurnawayConcurrentCount, institutionResources.FirstOrDefault(x => x.Id == mostTurnawayConcurrentId), InstitutionId, DateRangeStart, DateRangeEnd) : null;
            }
        }

        private void PopulateResourceLists(InstitutionDashboardStatistics stats,
            List<InstitutionResource> institutionResources)
        {
            if (stats.InstitutionResourceStatistics != null)
            {
                PurchasedList = institutionResources
                    .Where(z => stats.InstitutionResourceStatistics.Where(x => x.Purchased).Select(y => y.ResourceId)
                        .Contains(z.Id)).OrderByDescending(x => x.FirstPurchaseDate).Select(t =>
                        new DashboardResource(t, "Purchased", InstitutionId, DateRangeStart, DateRangeEnd)).ToList();
                ArchivedList = institutionResources
                    .Where(z => stats.InstitutionResourceStatistics.Where(x => x.ArchivedPurchased)
                        .Select(y => y.ResourceId).Contains(z.Id)).OrderByDescending(x => x.ReleaseDate).Select(t =>
                        new DashboardResource(t, "Archived", InstitutionId, DateRangeStart, DateRangeEnd)).ToList();
                NewEditionPurchasedList = institutionResources
                    .Where(z => stats.InstitutionResourceStatistics.Where(x => x.NewEditionPreviousPurchased)
                        .Select(y => y.ResourceId).Contains(z.Id)).OrderByDescending(x => x.ReleaseDate).Select(t =>
                        new DashboardResource(t, "NewEditionPurchased", InstitutionId, DateRangeStart, DateRangeEnd))
                    .ToList();

                PdaAddedList = institutionResources
                    .Where(z => stats.InstitutionResourceStatistics.Where(x => x.PdaAdded).Select(y => y.ResourceId)
                        .Contains(z.Id)).OrderByDescending(x => x.PdaCreatedDate).Select(t =>
                        new DashboardResource(t, "PdaAdded", InstitutionId, DateRangeStart, DateRangeEnd)).ToList();
                PdaAddedToCartList = institutionResources
                    .Where(z => stats.InstitutionResourceStatistics.Where(x => x.PdaAddedToCart)
                        .Select(y => y.ResourceId).Contains(z.Id)).OrderByDescending(x => x.PdaAddedToCartDate)
                    .Select(t =>
                        new DashboardResource(t, "PdaAddedToCart", InstitutionId, DateRangeStart, DateRangeEnd))
                    .ToList();
                PdaNewEditionList = institutionResources
                    .Where(z => stats.InstitutionResourceStatistics.Where(x => x.PdaNewEdition)
                        .Select(y => y.ResourceId).Contains(z.Id)).OrderByDescending(x => x.ReleaseDate).Select(t =>
                        new DashboardResource(t, "PdaNewEdition", InstitutionId, DateRangeStart, DateRangeEnd))
                    .ToList();


                var purchasedCount = PurchasedList.Count;
                var archivedCount = ArchivedList.Count;
                var newEditionPurchasedCount = NewEditionPurchasedList.Count;

                var pdaAddedCount = PdaAddedList.Count;
                var pdaAddedToCartCount = PdaAddedToCartList.Count;
                var pdaNewEditionCount = PdaNewEditionList.Count;

                //Limit display to 4 max
                PurchasedList = purchasedCount >= 4 ? PurchasedList.Take(4).ToList() :
                    purchasedCount > 2 ? PurchasedList.Take(2).ToList() : PurchasedList;
                ArchivedList = archivedCount >= 4 ? ArchivedList.Take(4).ToList() :
                    archivedCount > 2 ? ArchivedList.Take(2).ToList() : ArchivedList;
                NewEditionPurchasedList = newEditionPurchasedCount >= 4 ? NewEditionPurchasedList.Take(4).ToList() :
                    newEditionPurchasedCount > 2 ? NewEditionPurchasedList.Take(2).ToList() : NewEditionPurchasedList;
                PdaAddedList = pdaAddedCount >= 4 ? PdaAddedList.Take(4).ToList() :
                    PdaAddedList.Count > 2 ? PdaAddedList.Take(2).ToList() : PdaAddedList;
                PdaAddedToCartList = pdaAddedToCartCount >= 4 ? PdaAddedToCartList.Take(4).ToList() :
                    pdaAddedToCartCount > 2 ? PdaAddedToCartList.Take(2).ToList() : PdaAddedToCartList;
                PdaNewEditionList = pdaNewEditionCount >= 4 ? PdaNewEditionList.Take(4).ToList() :
                    pdaNewEditionCount > 2 ? PdaNewEditionList.Take(2).ToList() : PdaNewEditionList;
            }
        }

        public void PopulateContentSpotLight(List<InstitutionResource> featuredTitles,
            List<InstitutionResource> specialTitles,
            List<InstitutionResource> recommendedTitles)
        {
            FeaturedTitles = featuredTitles.Select(t =>
                new DashboardResource(t, "FeaturedTitles", InstitutionId, DateRangeStart, DateRangeEnd)).ToList();
            SpecialTitles = specialTitles
                .Select(t => new DashboardResource(t, "Specials", InstitutionId, DateRangeStart, DateRangeEnd))
                .ToList();
            RecommendedTitles = recommendedTitles.Select(t =>
                new DashboardResource(t, "Recommendations", InstitutionId, DateRangeStart, DateRangeEnd)).ToList();
        }
    }
}