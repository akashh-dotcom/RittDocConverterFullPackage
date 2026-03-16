#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Contexts;
using R2V2.Core.Institution;
using R2V2.Core.Reports;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Web.Areas.Admin.Services;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class ResourceUsageReportService : WebReportServiceBase
    {
        private readonly IAdminContext _adminContext;
        private readonly IAuthenticationContext _authenticationContext;
        private readonly IResourceService _resourceService;
        private readonly IUserSessionStorageService _userSessionStorageService;

        /// <param name="userSessionStorageService"> </param>
        ///// <param name="counterReportService"> </param>
        public ResourceUsageReportService(ILog<ReportServiceBase> log
            , IQueryable<SavedReport> savedReports
            , IReportService reportService
            , IUserSessionStorageService userSessionStorageService
            , IAuthenticationContext authenticationContext
            , IAdminContext adminContext
            , IResourceService resourceService
            , IpAddressRangeService ipAddressRangeService
        )
            : base(log, savedReports, reportService, adminContext, ipAddressRangeService)
        {
            _userSessionStorageService = userSessionStorageService;
            _authenticationContext = authenticationContext;
            _adminContext = adminContext;
            _resourceService = resourceService;
        }

        public List<ResourceReportItem> GetResourceUsageReportData(ReportModel reportModel)
        {
            ReportRequest.IsPublisherUser = _authenticationContext.AuthenticatedInstitution.IsPublisherUser();

            if (!ReportRequest.IsPublisherUser)
            {
                var adminInstitution = _adminContext.GetAdminInstitution(reportModel.InstitutionId);

                ReportRequest.IsTrialAccount = adminInstitution.AccountStatus != null &&
                                               (adminInstitution.AccountStatus.Id == AccountStatus.Trial ||
                                                adminInstitution.AccountStatus.Id == AccountStatus.TrialExpired);
            }
            else if (ReportRequest.PublisherId == 0)
            {
                ReportRequest.PublisherId = _authenticationContext.AuthenticatedInstitution.Publisher.Id;
            }

            var page = reportModel.ReportQuery.Page;

            var items = _userSessionStorageService.Get<List<ResourceReportItem>>(ResourceUsageReportKey);
            if (items == null || page < 2)
            {
                var resources = _resourceService.GetAllResources().ToList();
                items = ReportService.GetResourceReportItems(ReportRequest, resources);

                _userSessionStorageService.Put(ResourceUsageReportKey, items);
            }

            return items;
        }
    }
}