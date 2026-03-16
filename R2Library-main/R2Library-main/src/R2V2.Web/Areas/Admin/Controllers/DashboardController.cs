#region

using System;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Reports;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.Dashboard;
using R2V2.Web.Areas.Admin.Services;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    [RequiresInstitutionId]
    public class DashboardController : R2AdminBaseController
    {
        private readonly RecentCookieService _recentCookieService;
        private readonly WebDashboardService _webDashboardService;

        public DashboardController(
            IAuthenticationContext authenticationContext
            , WebDashboardService webDashboardService
            , RecentCookieService recentCookieService
        )
            : base(authenticationContext)
        {
            _webDashboardService = webDashboardService;
            _recentCookieService = recentCookieService;
        }

        public ActionResult Index(DashboardModel dashboardModel)
        {
            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                dashboardModel.InstitutionId != CurrentUser.InstitutionId)
            {
                dashboardModel.InstitutionId = CurrentUser.InstitutionId.GetValueOrDefault();
                return RedirectToAction("Index", new { dashboardModel.InstitutionId });
            }

            SetDateRanges(dashboardModel);

            dashboardModel.Period = GetPeriod(dashboardModel);

            var model = _webDashboardService.GetBaseDashBoard(dashboardModel.InstitutionId,
                dashboardModel.DateRangeStart, dashboardModel.DateRangeEnd);

            model.Period = dashboardModel.Period;
            model.SetPageLinks();
            model.HighLightLink.Selected = true;

            if ((IsRittenhouseAdmin() || IsSalesAssociate()) && dashboardModel.InstitutionId > 0)
            {
                _recentCookieService.SetRecentInstitutionsCookie(dashboardModel.InstitutionId, Response, Request);
            }

            return View(model);
        }

        public ActionResult EbookCollection(DashboardModel dashboardModel)
        {
            SetDateRanges(dashboardModel);

            dashboardModel.Period = GetPeriod(dashboardModel);

            var model = _webDashboardService.GetEbookDashBoard(dashboardModel.InstitutionId,
                dashboardModel.DateRangeStart, dashboardModel.DateRangeEnd);

            model.Period = dashboardModel.Period;
            model.SetPageLinks();
            model.EbookCollectionLink.Selected = true;

            if ((IsRittenhouseAdmin() || IsSalesAssociate()) && dashboardModel.InstitutionId > 0)
            {
                _recentCookieService.SetRecentInstitutionsCookie(dashboardModel.InstitutionId, Response, Request);
            }

            return View(model);
        }

        public ActionResult PdaCollection(DashboardModel dashboardModel)
        {
            SetDateRanges(dashboardModel);

            dashboardModel.Period = GetPeriod(dashboardModel);

            var model = _webDashboardService.GetPdaDashBoard(dashboardModel.InstitutionId,
                dashboardModel.DateRangeStart, dashboardModel.DateRangeEnd);

            model.Period = dashboardModel.Period;
            model.SetPageLinks();
            model.PdaCollectionLink.Selected = true;

            if ((IsRittenhouseAdmin() || IsSalesAssociate()) && dashboardModel.InstitutionId > 0)
            {
                _recentCookieService.SetRecentInstitutionsCookie(dashboardModel.InstitutionId, Response, Request);
            }

            return View(model);
        }

        /// <summary>
        ///     Will only set the DateRangeStart && DateRangeEnd if they are minimum values
        /// </summary>
        private void SetDateRanges(DashboardModel model)
        {
            DateTime startDate;
            DateTime endDate;
            switch (model.Period)
            {
                case ReportPeriod.LastTwelveMonths:
                    model.SetStartEndDate(ReportPeriod.LastTwelveMonths, out startDate, out endDate);
                    break;
                case ReportPeriod.LastSixMonths:
                    model.SetStartEndDate(ReportPeriod.LastSixMonths, out startDate, out endDate);
                    break;
                case ReportPeriod.UserSpecified:
                    model.SetStartEndDate(ReportPeriod.UserSpecified, out startDate, out endDate);
                    break;
                case ReportPeriod.CurrentYear:
                    model.SetStartEndDate(ReportPeriod.CurrentYear, out startDate, out endDate);
                    break;
                case ReportPeriod.LastYear:
                    model.SetStartEndDate(ReportPeriod.LastYear, out startDate, out endDate);
                    break;
                default:
                case ReportPeriod.PreviousMonth:
                    model.SetStartEndDate(ReportPeriod.PreviousMonth, out startDate, out endDate);
                    break;
            }

            if (model.Period == ReportPeriod.Today && model.DateRangeStart != DateTime.MinValue &&
                model.DateRangeEnd != DateTime.MinValue)
            {
            }
            else
            {
                model.DateRangeStart = startDate;
                model.DateRangeEnd = endDate;
            }
        }

        private ReportPeriod GetPeriod(DashboardModel dashboardModel)
        {
            DateTime startDate;
            DateTime endDate;
            dashboardModel.SetStartEndDate(ReportPeriod.PreviousMonth, out startDate, out endDate);
            if (dashboardModel.DateRangeStart == startDate && dashboardModel.DateRangeEnd == endDate)
            {
                return ReportPeriod.PreviousMonth;
            }

            dashboardModel.SetStartEndDate(ReportPeriod.LastTwelveMonths, out startDate, out endDate);
            if (dashboardModel.DateRangeStart == startDate && dashboardModel.DateRangeEnd == endDate)
            {
                return ReportPeriod.LastTwelveMonths;
            }

            dashboardModel.SetStartEndDate(ReportPeriod.LastSixMonths, out startDate, out endDate);
            if (dashboardModel.DateRangeStart == startDate && dashboardModel.DateRangeEnd == endDate)
            {
                return ReportPeriod.LastSixMonths;
            }

            dashboardModel.SetStartEndDate(ReportPeriod.CurrentYear, out startDate, out endDate);
            if (dashboardModel.DateRangeStart == startDate && dashboardModel.DateRangeEnd == endDate)
            {
                return ReportPeriod.CurrentYear;
            }

            dashboardModel.SetStartEndDate(ReportPeriod.LastYear, out startDate, out endDate);
            if (dashboardModel.DateRangeStart == startDate && dashboardModel.DateRangeEnd == endDate)
            {
                return ReportPeriod.LastYear;
            }

            return ReportPeriod.UserSpecified;
        }
    }
}