#region

using System.Linq;
using R2V2.Core.Reports;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Storages;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class PublisherUsageReportService
    {
        protected const string PublisherUsageReportKey = "Report.Publisher.User.Data";

        private readonly IReportService _reportService;
        private readonly IResourceService _resourceService;
        private readonly IUserSessionStorageService _userSessionStorageService;
        private ReportRequest _reportRequest;

        public PublisherUsageReportService(IReportService reportService, IResourceService resourceService,
            IUserSessionStorageService userSessionStorageService)
        {
            _reportService = reportService;
            _resourceService = resourceService;
            _userSessionStorageService = userSessionStorageService;
        }

        public PublisherReportCounts RunPublisherReportCounts(ReportQuery publisherReportQuery)
        {
            var page = publisherReportQuery.Page;

            var publisherReport = _userSessionStorageService.Get<PublisherReportCounts>(PublisherUsageReportKey);
            if (publisherReport == null || page < 2)
            {
                SetReportRequest(publisherReportQuery);
                var resources = _resourceService.GetAllResources().ToList();

                publisherReport = _reportService.GetPublisherReportCounts(_reportRequest, resources);

                _userSessionStorageService.Put(PublisherUsageReportKey, publisherReport);
                publisherReport.StartDate = _reportRequest.DateRangeStart;
                publisherReport.EndDate = _reportRequest.DateRangeEnd;
            }
            //return items;


            return publisherReport;
        }

        public void SetReportRequest(ReportQuery publisherReportQuery)
        {
            _reportRequest = new ReportRequest
            {
                DateRangeEnd = publisherReportQuery.DateRangeEnd.GetValueOrDefault(),
                DateRangeStart = publisherReportQuery.DateRangeStart.GetValueOrDefault(),
                PublisherId = publisherReportQuery.PublisherId,
                Period = (ReportPeriod)publisherReportQuery.Period,
                Type = ReportType.PublisherUser
            };
        }
    }
}