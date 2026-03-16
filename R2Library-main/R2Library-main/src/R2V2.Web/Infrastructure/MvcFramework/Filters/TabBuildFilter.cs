#region

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Controllers;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.Report;
using R2V2.Web.Controllers;
using R2V2.Web.Controllers.MyR2;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public class TabBuildFilter : R2V2ResultFilter
    {
        private readonly IAdminContext _adminContext;
        private readonly IAdminSettings _adminSettings;
        private readonly IAuthenticationContext _authenticationContext;
        private readonly IInstitutionSettings _institutionSettings;
        private readonly ILog<TabBuildFilter> _log;

        public TabBuildFilter(ILog<TabBuildFilter> log
            , IAuthenticationContext authenticationContext
            , IAdminSettings adminSettings
            , IInstitutionSettings institutionSettings
            , IAdminContext adminContext
        ) : base(authenticationContext)
        {
            _log = log;
            _authenticationContext = authenticationContext;
            _adminSettings = adminSettings;
            _institutionSettings = institutionSettings;
            _adminContext = adminContext;
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var controller = filterContext.Controller;
            var model = controller.ViewData.Model;
            var baseModel = model as IR2V2Model;
            if (baseModel == null)
            {
                return;
            }

            var tabs = new SortedList<int, HeaderTab>();
            var urlHelper = new UrlHelper(filterContext.RequestContext);

            AddBrowseTab(tabs, controller, urlHelper);

            AddAtoZIndexTab(tabs, controller, urlHelper);

            AddMyR2Tab(tabs, controller, urlHelper);

            AddReserveShelfTab(tabs, controller, urlHelper);

            AddSubscriptionTab(tabs, controller, urlHelper);

            AddReportsTab(tabs, controller, urlHelper);

            AddBackTabButton(tabs, controller, urlHelper);

            AddAdminTabButton(tabs, controller, urlHelper);

            baseModel.Tabs = tabs;
        }

        public void AddAdminTabButton(SortedList<int, HeaderTab> tabs, ControllerBase controller, UrlHelper urlHelper)
        {
            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;

            if (controller is IR2AdminBaseController)
            {
                return;
            }

            if (authenticatedInstitution != null)
            {
                if (_adminSettings.DisplayAdminTab)
                {
                    var isInstitutionAdmin = authenticatedInstitution.IsInstitutionAdmin();
                    var isRittenhouseAdmin = authenticatedInstitution.IsRittenhouseAdmin();
                    var isSalesAssociate = authenticatedInstitution.IsSalesAssociate();
                    var isExpertReviewer = authenticatedInstitution.IsExpertReviewer();
                    try
                    {
                        if (isRittenhouseAdmin || isInstitutionAdmin || isSalesAssociate)
                        {
                            var adminTab = new HeaderTab
                            {
                                DisplayText = "Admin",
                                IsSelected = controller is IR2AdminBaseController,
                                Url =
                                    isRittenhouseAdmin || isSalesAssociate
                                        ? urlHelper.AdminAction<InstitutionController>(a => a.List(null))
                                        : $"/Admin/Dashboard/Index/{authenticatedInstitution.Id}"
                            };

                            tabs.Add(120, adminTab);
                        }
                        else if (isExpertReviewer)
                        {
                            var adminInsitution = _adminContext.GetAdminInstitution(authenticatedInstitution.Id);
                            if (adminInsitution.ExpertReviewerUserEnabled)
                            {
                                var adminTab = new HeaderTab
                                {
                                    DisplayText = "Expert",
                                    IsSelected = controller is IR2AdminBaseController,
                                    Url = $"/Admin/CollectionManagement/List/{authenticatedInstitution.Id}"
                                };
                                tabs.Add(120, adminTab);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = new StringBuilder();
                        msg.AppendFormat(
                            "isRittenhouseAdmin: {0}, isInstitutionAdmin: {1}, isSalesAssociate: {2}, isExpertReviewer: {3}",
                            isRittenhouseAdmin, isInstitutionAdmin, isSalesAssociate, isExpertReviewer);
                        msg.AppendLine()
                            .AppendFormat("Institution: [Id: {0}, Account Number: {1}, Name: {2}]",
                                authenticatedInstitution.Id,
                                authenticatedInstitution.AccountNumber,
                                authenticatedInstitution.Name)
                            .AppendLine();
                        msg.Append(ex.Message);
                        _log.Error(msg.ToString(), ex);
                        throw;
                    }
                }
            }
        }

        public void AddBackTabButton(SortedList<int, HeaderTab> tabs, ControllerBase controller, UrlHelper urlHelper)
        {
            if (controller is IR2AdminBaseController)
            {
                var backTab = new HeaderTab
                {
                    DisplayText = "<< Back to R2Library",
                    IsSelected = false,
                    Url = "/"
                };
                tabs.Add(110, backTab);
            }
        }


        public void AddBrowseTab(SortedList<int, HeaderTab> tabs, ControllerBase controller, UrlHelper urlHelper)
        {
            if (controller is IR2AdminBaseController)
            {
                return;
            }

            var browseTab = new HeaderTab
            {
                DisplayText = "Browse",
                IsSelected = controller is BrowseController,
                Url = urlHelper.Action<BrowseController>(a => a.Index())
            };

            tabs.Add(10, browseTab);
        }

        public void AddAtoZIndexTab(SortedList<int, HeaderTab> tabs, ControllerBase controller, UrlHelper urlHelper)
        {
            if (controller is IR2AdminBaseController)
            {
                return;
            }

            var alphaIndexTab = new HeaderTab
            {
                DisplayText = "A-Z Index",
                IsSelected = controller is AlphaIndexController,
                Url = urlHelper.Action<AlphaIndexController>(a => a.Index())
            };
            tabs.Add(20, alphaIndexTab);
        }

        public void AddMyR2Tab(SortedList<int, HeaderTab> tabs, ControllerBase controller, UrlHelper urlHelper)
        {
            if (controller is IR2AdminBaseController)
            {
                return;
            }

            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            if (UserId > 0 || (authenticatedInstitution != null && authenticatedInstitution.Id > 0))
            {
                var myR2Tab = new HeaderTab
                {
                    DisplayText = "My R2",
                    IsSelected = controller is MyR2Controller || controller is ProfileController,
                    Url = urlHelper.Action<MyR2Controller>(a => a.Index("bookmarks"))
                };
                tabs.Add(30, myR2Tab);
            }
        }


        public void AddReserveShelfTab(SortedList<int, HeaderTab> tabs, ControllerBase controller, UrlHelper urlHelper)
        {
            if (controller is IR2AdminBaseController)
            {
                return;
            }

            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            if (authenticatedInstitution != null &&
                authenticatedInstitution.AccountNumber != _institutionSettings.GuestAccountNumber &&
                authenticatedInstitution.HasReservedShelf)
            {
                var reserveShelfTab = new HeaderTab
                {
                    DisplayText = "Reserve Shelf",
                    IsSelected = controller is ReserveShelfController,
                    Url = urlHelper.Action<ReserveShelfController>(a => a.Index(0))
                };
                tabs.Add(40, reserveShelfTab);
            }
        }

        public void AddReportsTab(SortedList<int, HeaderTab> tabs, ControllerBase controller, UrlHelper urlHelper)
        {
            if (controller is IR2AdminBaseController)
            {
                return;
            }

            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            if (authenticatedInstitution != null && authenticatedInstitution.IsPublisherUser())
            {
                var query = new ReportQuery { PublisherId = authenticatedInstitution.Publisher.Id, ReportId = 0 };
                var reportsTab = new HeaderTab
                {
                    DisplayText = "Reports",
                    IsSelected = false,
                    Url = urlHelper.Action("ResourceUsage", "Report", query.ToRouteValues())
                };
                tabs.Add(50, reportsTab);
            }
        }


        public void AddSubscriptionTab(SortedList<int, HeaderTab> tabs, ControllerBase controller, UrlHelper urlHelper)
        {
            if (controller is IR2AdminBaseController)
            {
                return;
            }

            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            if (!_institutionSettings.HideSubscriptionsTab)
            {
                if (authenticatedInstitution == null || _authenticationContext.IsSubscriptionUser())
                {
                    var subscriptionsTab = new HeaderTab
                    {
                        DisplayText = "Subscriptions",
                        IsSelected = controller is SubscriptionsController,
                        Url = urlHelper.Action<SubscriptionsController>(a => a.Index(0))
                    };
                    tabs.Add(_authenticationContext.IsSubscriptionUser() ? 40 : 30, subscriptionsTab);
                }
            }

            if (authenticatedInstitution == null && !string.IsNullOrWhiteSpace(_adminSettings.PublicCollectionTabName))
            {
                var collectionsTab = new HeaderTab
                {
                    DisplayText = _adminSettings.PublicCollectionTabName,
                    IsSelected = controller is CollectionsController,
                    Url = urlHelper.Action<CollectionsController>(a => a.Index(null, null))
                };
                tabs.Add(50, collectionsTab);
            }
        }
    }
}