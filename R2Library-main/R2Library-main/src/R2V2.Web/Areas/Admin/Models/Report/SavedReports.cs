#region

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using R2V2.Core.Admin;
using R2V2.Core.Publisher;
using R2V2.Core.Reports;
using R2V2.Core.Resource;
using R2V2.Core.Resource.PracticeArea;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class SavedReports : ReportModel
    {
        private readonly List<SavedReportListItem> _savedReportsItems = new List<SavedReportListItem>();

        /// <summary>
        /// </summary>
        public SavedReports()
        {
        }

        public SavedReports(IAdminInstitution institution) : base(institution)
        {
        }

        public IEnumerable<SavedReportListItem> SavedReportItems => _savedReportsItems;

        /// <summary>
        /// </summary>
        /// <param name="requestContext"> </param>
        public void AddSavedReports(IEnumerable<SavedReport> list, RequestContext requestContext, int institutionId
            , List<IPracticeArea> practiceAreas, List<IPublisher> publishers, List<IResource> resources)
        {
            var urlHelper = new UrlHelper(requestContext);

            _savedReportsItems.Clear();

            foreach (var savedReport in list)
            {
                var item = new SavedReportListItem
                {
                    CreationDate = savedReport.CreationDate,
                    Description = savedReport.Description,
                    Email = savedReport.Email,
                    Id = savedReport.Id,
                    Frequency = (ReportFrequency)savedReport.Frequency,
                    LastUpdate = savedReport.LastUpdate,
                    Name = savedReport.Name,
                    ReportType = (ReportType)savedReport.Type,
                    ExecuteLink = GetExecuteLink(savedReport, urlHelper),
                    DetailLink = GetDetailLink(savedReport, urlHelper),
                    DeleteLink = GetDeleteLink(savedReport, urlHelper),
                    AdminDeleteLink = GetAdminDeleteLink(savedReport, urlHelper),
                    Period = savedReport.Period,
                    PeriodStartDate = savedReport.PeriodStartDate,
                    PeriodEndDate = savedReport.PeriodEndDate,
                    IncludePurchased = savedReport.IncludePurchased,
                    IncludePda = savedReport.IncludePda,
                    IncludeToc = savedReport.IncludeToc,
                    IncludeTrialStats = savedReport.IncludeTrialStats
                };

                if (savedReport.PublisherId > 0)
                {
                    var publisher = publishers.FirstOrDefault(x => x.Id == savedReport.PublisherId);
                    if (publisher != null)
                    {
                        item.PublisherDisplay = publisher.DisplayName ?? publisher.Name;
                    }
                }

                if (savedReport.ResourceId > 0)
                {
                    var resource = resources.FirstOrDefault(x => x.Id == savedReport.ResourceId);
                    if (resource != null)
                    {
                        item.ResourceDisplay = $"{resource.Title} ({resource.Isbn})";
                    }
                }

                if (savedReport.PracticeAreaId > 0)
                {
                    item.PracticeAreaDisplay =
                        practiceAreas.FirstOrDefault(x => x.Id == savedReport.PracticeAreaId)?.Name;
                }


                if (institutionId == 0)
                {
                    item.InstitutionAccountNumber = savedReport.Institution.AccountNumber;
                    item.InstitutionName = savedReport.Institution.Name;
                    item.InstitutionId = savedReport.Institution.Id;
                }

                _savedReportsItems.Add(item);
            }
        }

        public string GetFrequency(int frequencyId)
        {
            if (frequencyId == 7)
            {
                return "Weekly";
            }

            if (frequencyId == 30)
            {
                return "Monthly";
            }

            return frequencyId == 14 ? "Bi-Weekly" : "Not Scheduled";
        }


        public string GetExecuteLink(SavedReport savedReport, UrlHelper urlHelper)
        {
            var reportQuery = new ReportQuery
            {
                ReportId = savedReport.Id,
                InstitutionId = savedReport.InstitutionId,
                Page = 1
            };

            return
                urlHelper.Action(
                    savedReport.Type == (int)ReportType.ResourceUsageReport ? "ResourceUsage" : "ApplicationUsage",
                    "Report",
                    reportQuery.ToRouteValues());
        }

        public string GetDetailLink(SavedReport savedReport, UrlHelper urlHelper)
        {
            //ReportQuery reportQuery = new ReportQuery {ReportId = savedReport.Id, InstitutionId = savedReport.InstitutionId};
            //return urlHelper.AdminAction<ReportController>(a => a.SavedReport(reportQuery));
            // TODO - SJS 11/29/2012 - temp solution
            return $"/Admin/Report/SavedReport/{savedReport.InstitutionId}?reportid={savedReport.Id}";
        }

        public string GetDeleteLink(SavedReport savedReport, UrlHelper urlHelper)
        {
            //ReportQuery reportQuery = new ReportQuery {ReportId = savedReport.Id, InstitutionId = savedReport.InstitutionId};
            //return urlHelper.AdminAction<ReportController>(a => a.SavedReport(reportQuery));
            // TODO - SJS 11/29/2012 - temp solution
            return $"/Admin/Report/DeleteSavedReport/{savedReport.InstitutionId}?reportid={savedReport.Id}";
        }

        /// <summary>
        ///     This is Needed so RAs do not get pushed to the institution when they delete reports.
        /// </summary>
        public string GetAdminDeleteLink(SavedReport savedReport, UrlHelper urlHelper)
        {
            return $"/Admin/Report/DeleteSavedReport/{0}?reportid={savedReport.Id}";
        }
    }
}