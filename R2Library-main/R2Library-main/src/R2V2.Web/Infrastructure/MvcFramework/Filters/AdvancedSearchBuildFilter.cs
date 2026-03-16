#region

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Infrastructure.Settings;
using R2V2.Web.Areas.Admin.Controllers;
using R2V2.Web.Areas.Admin.Controllers.CollectionManagement;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Models;
using R2V2.Web.Models.Search;
using ResourceController = R2V2.Web.Areas.Admin.Controllers.ResourceController;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public class AdvancedSearchBuildFilter : R2V2ResultFilter
    {
        private readonly IContentSettings _contentSettings;
        private readonly Func<ResourceService> _resourceServiceFactory;

        /// <param name="authenticationContext"> </param>
        public AdvancedSearchBuildFilter(IAuthenticationContext authenticationContext,
            Func<ResourceService> resourceServiceFactory, IContentSettings contentSettings)
            : base(authenticationContext)
        {
            _resourceServiceFactory = resourceServiceFactory;
            _contentSettings = contentSettings;
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            //_log.Debug("OnResultExecuting()");
            var model = filterContext.Controller.ViewData.Model;
            var baseModel = model as IR2V2Model;

            if (baseModel == null)
            {
                return;
            }

            var urlHelper = new UrlHelper(filterContext.RequestContext);

            var controller = filterContext.Controller;
            if (controller is IR2AdminBaseController)
            {
                baseModel.SearchUrl = "";
                baseModel.DisplayAdvancedSearch = false;
                if (controller is UserController)
                {
                    baseModel.SearchHintPlaceHolderText = "Search for users by name or email address";
                    baseModel.DisplaySearch = true;
                    baseModel.SearchUrl = urlHelper.Action("List", "User");
                }
                else if (controller is CollectionManagementController)
                {
                    baseModel.SearchHintPlaceHolderText = "Search for resources by title, author, ISBN or key word";
                    baseModel.DisplaySearch = true;
                    baseModel.SearchUrl = urlHelper.Action("List", "CollectionManagement");
                }
                else if (controller is InstitutionController)
                {
                    if (AuthenticationContext.IsRittenhouseAdmin() || AuthenticationContext.IsSalesAssociate())
                    {
                        baseModel.SearchHintPlaceHolderText = "Search for institutions by name or account number";
                        baseModel.DisplaySearch = true;
                        baseModel.SearchUrl = urlHelper.Action("List", "Institution");
                    }
                }
                else if (controller is ResourceController)
                {
                    baseModel.SearchHintPlaceHolderText = "Search for resources by title, author, ISBN or key word";
                    baseModel.DisplaySearch = true;
                    baseModel.SearchUrl = urlHelper.Action("List", "Resource");
                }
                else if (controller is ReviewController)
                {
                    var actionName = filterContext.RouteData.Values["action"].ToString();
                    if (actionName == "Resources")
                    {
                        baseModel.SearchHintPlaceHolderText = "Search for resources by title, author, ISBN or key word";
                        baseModel.DisplaySearch = true;
                        baseModel.SearchUrl = urlHelper.Action("Resources", "Review");
                    }
                }
                else if (controller is ReserveShelfManagementController)
                {
                    var actionName = filterContext.RouteData.Values["action"].ToString();
                    if (actionName == "ManageResources")
                    {
                        baseModel.SearchHintPlaceHolderText = "Search for resources by title, author, ISBN or key word";
                        baseModel.DisplaySearch = true;
                        baseModel.SearchUrl = urlHelper.Action("ManageResources", "ReserveShelfManagement");
                    }
                }
                else
                {
                    baseModel.DisplaySearch = false;
                }

                return;
            }

            if (baseModel.AdvancedSearch == null)
            {
                baseModel.AdvancedSearch = new AdvancedSearchModel();
            }

            var display = filterContext.Controller as IInstitutionDisplay;
            baseModel.AdvancedSearch.DisplayTocAvailable = display != null
                ? display.DisplayTocAvailable()
                : AuthenticationContext.DisplayTocAvailable();

            var resourceService = _resourceServiceFactory();
            baseModel.AdvancedSearch.PublicationYears = resourceService.GetResourcePublicationYears();
            baseModel.AdvancedSearch.PracticeAreas = new List<PracticeArea>();
            baseModel.SearchHintPlaceHolderText = "Search content for a drug, publication title, or condition";
            baseModel.DisplaySearch = true;
            baseModel.DisplayAdvancedSearch = true;
            baseModel.SearchTypeaheadEnabled = _contentSettings.SearchTypeaheadResultLimit > 0;
            baseModel.SearchUrl = urlHelper.Action("Index", "Search");
        }
    }
}