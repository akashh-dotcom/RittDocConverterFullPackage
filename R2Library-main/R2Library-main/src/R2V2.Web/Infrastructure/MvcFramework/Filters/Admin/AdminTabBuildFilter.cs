#region

using System.Collections.Generic;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.Reports;
using R2V2.Web.Areas.Admin.Controllers;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.Report;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters.Admin
{
    public class AdminTabBuildFilter : R2V2ResultFilter
    {
        public AdminTabBuildFilter(IAuthenticationContext authenticationContext) : base(authenticationContext)
        {
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var controller = filterContext.Controller;
            var model = controller.ViewData.Model;
            var adminBaseModel = model as AdminBaseModel;
            if (adminBaseModel == null)
            {
                return;
            }

            adminBaseModel.IsRittenhouseAdmin = AuthenticationContext.IsRittenhouseAdmin();
            adminBaseModel.IsInstitutionalAdmin = AuthenticationContext.IsInstitutionAdmin();
            adminBaseModel.IsPublisherUser = AuthenticationContext.IsPublisherUser();
            adminBaseModel.IsSalesAssociate = AuthenticationContext.IsSalesAssociate();
            adminBaseModel.IsExpertReviewer = AuthenticationContext.IsExpertReviewer();

            var urlHelper = new UrlHelper(filterContext.RequestContext);

            if (controller is ConfigurationController)
            {
                var actionName = filterContext.RouteData.Values["action"].ToString();
                adminBaseModel.TabLinks = BuildAdminTabLinks(urlHelper, actionName, adminBaseModel);
            }
            else if (AuthenticationContext.IsRittenhouseAdmin() || AuthenticationContext.IsSalesAssociate())
            {
                adminBaseModel.TabLinks = BuildTabLinks(urlHelper, controller, adminBaseModel);
            }
        }

        /// <summary>
        ///     Both RA and sales associate users should have access to the institutions, user & reports menus
        ///     Only RA users have access to Resources and Publishers
        /// </summary>
        public IEnumerable<PageLink> BuildTabLinks(UrlHelper urlHelper, ControllerBase controller, AdminBaseModel model)
        {
            yield return new PageLink
            {
                Selected = controller is InstitutionController ||
                           model.InstitutionId >
                           0, //(model.Institution != null && !string.IsNullOrWhiteSpace(model.Institution.AccountNumber)),
                Href = urlHelper.Action("List", "Institution"),
                Text = "Institutions"
            };

            if (AuthenticationContext.IsRittenhouseAdmin())
            {
                yield return new PageLink
                {
                    Selected = controller is ResourceController, Href = urlHelper.Action("List", "Resource"),
                    Text = "Resources"
                };
            }

            yield return new PageLink
            {
                Selected = controller is UserController && (model.Institution == null ||
                                                            string.IsNullOrWhiteSpace(model.Institution.AccountNumber)),
                Href = urlHelper.Action("List", "User", new UserQuery { InstitutionId = 0 }.ToRouteValues()),
                Text = "Users"
            };

            yield return new PageLink
            {
                Selected = controller is ReportController && model.InstitutionId == 0,
                Href = urlHelper.Action("ApplicationUsage", "Report",
                    new ReportQuery { Period = ReportPeriod.Last30Days }.ToAdminRouteValues(0)),
                Text = "Reports"
            };

            //yield return new PageLink { Selected = controller is MarketingController, Href = urlHelper.Action("AutomatedCart", "Marketing"), Text = "Marketing" };

            if (AuthenticationContext.IsRittenhouseAdmin())
            {
                yield return new PageLink
                {
                    Selected = controller is PublisherController, Href = urlHelper.Action("List", "Publisher"),
                    Text = "Publishers"
                };
                yield return new PageLink
                {
                    Selected = controller is AlertController, Href = urlHelper.Action("List", "Alert"), Text = "Alerts"
                };

                //yield return
                //    new PageLink
                //    {
                //        Selected = (controller is PromotionController || controller is SpecialController || controller is PdaPromotionController),
                //        Href = urlHelper.Action("List", "Promotion"),
                //        Text = "Discounts"
                //    };

                if (AuthenticationContext.AuthenticatedInstitution.User.EnablePromotion != null &&
                    AuthenticationContext.AuthenticatedInstitution.User.EnablePromotion.Value > 0)
                {
                    yield return
                        new PageLink
                        {
                            Selected = controller is ResourcePromotionController,
                            Href = urlHelper.Action("Queue", "ResourcePromotion"),
                            Text = "Resource Promotion"
                        };
                }

                yield return new PageLink
                {
                    Selected = controller is SpecialCollectionManagementController,
                    Href = urlHelper.Action("List", "SpecialCollectionManagement"), Text = "Special Collections"
                };

                yield return new PageLink
                {
                    Selected = controller is MarketingController || controller is SubscriptionManagementController ||
                               controller is PromotionController || controller is SpecialController ||
                               controller is PdaPromotionController,
                    Href = urlHelper.Action("List", "Promotion"),
                    Text = "Misc."
                };
                //if (controller is PromotionController || controller is SpecialController || controller is PdaPromotionController)
            }
        }

        public IEnumerable<PageLink> BuildAdminTabLinks(UrlHelper urlHelper, string actionResult, AdminBaseModel model)
        {
            yield return new PageLink
            {
                Selected = false,
                Href = urlHelper.Action("List", "Institution"),
                Text = "Back To Admin"
            };

            actionResult = actionResult == null ? "" : actionResult.ToLower();

            yield return new PageLink
            {
                Selected = actionResult == "cache",
                Href = urlHelper.Action("Cache", "Configuration"),
                Text = "Cached Items"
            };

            yield return new PageLink
            {
                Selected = actionResult == "configurationgrouplist" || actionResult == "configurationgroupsettings",
                Href = urlHelper.Action("ConfigurationGroupList", "Configuration"),
                Text = "Configuration Group Settings"
            };

            yield return new PageLink
            {
                Selected = actionResult == "liveconfigurationsettings",
                Href = urlHelper.Action("LiveConfigurationSettings", "Configuration"),
                Text = "Live Configuration Settings"
            };
        }
    }
}