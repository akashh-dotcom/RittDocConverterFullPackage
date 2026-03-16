#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Core.Territory;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.Menus;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters.Admin
{
    public class AdminActionsMenuBuildFilter : R2V2ResultFilter
    {
        private readonly Func<IAdminContext> _adminContextFactory;
        private readonly IAuthenticationContext _authenticationContext;
        private readonly Func<ICartService> _cartServiceFactory;
        private readonly Func<ICollectionService> _collectionServiceFactory;
        private readonly Func<IInstitutionTypeService> _institutionTypeService;
        private readonly Func<IPracticeAreaService> _practiceAreaServiceFactory;
        private readonly Func<ISpecialtyService> _specialtyServiceFactory;
        private readonly Func<ITerritoryService> _territoryService;
        private readonly IWebSettings _webSettings;

        public AdminActionsMenuBuildFilter(IAuthenticationContext authenticationContext,
            Func<IPracticeAreaService> practiceAreaServiceFactory,
            Func<ISpecialtyService> specialtyServiceFactory,
            Func<ICollectionService> collectionServiceFactory,
            Func<ITerritoryService> territoryService,
            Func<IInstitutionTypeService> institutionTypeService,
            Func<ICartService> carttServiceFactory,
            Func<IAdminContext> adminContextFactory,
            IWebSettings webSettings
        )
            : base(authenticationContext)
        {
            _authenticationContext = authenticationContext;
            _practiceAreaServiceFactory = practiceAreaServiceFactory;
            _specialtyServiceFactory = specialtyServiceFactory;
            _collectionServiceFactory = collectionServiceFactory;
            _territoryService = territoryService;
            _institutionTypeService = institutionTypeService;
            _cartServiceFactory = carttServiceFactory;
            _adminContextFactory = adminContextFactory;
            _webSettings = webSettings;
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

            var urlHelper = new UrlHelper(filterContext.RequestContext);

            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;

            var actionsMenuBuilder = ActionsMenuBuilderFactory.CreateActionsMenuBuilder(controller,
                _practiceAreaServiceFactory(), _specialtyServiceFactory(), _collectionServiceFactory(),
                _territoryService(), _webSettings, _collectionServiceFactory(), _institutionTypeService());
            var actionsMenu = actionsMenuBuilder.Build(authenticatedInstitution, urlHelper, adminBaseModel);

            var institutionId = adminBaseModel.InstitutionId;
            if (institutionId > 0)
            {
                var adminInstitution = _adminContextFactory().GetAdminInstitution(institutionId);
                if (adminInstitution != null)
                {
                    actionsMenu.InstitutionId = adminInstitution.Id;
                    var cart = _cartServiceFactory().GetInstitutionCartFromCache(adminBaseModel.InstitutionId);
                    var allCachedCarts = _cartServiceFactory()
                        .GetAllInstitutionCartsFromCache(adminBaseModel.InstitutionId);

                    if (cart != null)
                    {
                        actionsMenu.CartId = cart.Id;
                        actionsMenu.CartTotal = cart.Total;
                        actionsMenu.CartItemCount = cart.ItemCount;
                        actionsMenu.DisplayCartLink = cart.ItemCount > 0 &&
                                                      (authenticatedInstitution.IsInstitutionAdmin() ||
                                                       authenticatedInstitution.IsRittenhouseAdmin() ||
                                                       authenticatedInstitution.IsSalesAssociate());
                        actionsMenu.DisplaySavedCartLink = true;
                        actionsMenu.SavedCartsCount = authenticatedInstitution.IsRittenhouseAdmin()
                            ? allCachedCarts.Count(x => x.CartType != CartTypeEnum.Active)
                            : allCachedCarts.Count(x => x.CartType != CartTypeEnum.Active && x.ResellerId == 0);
                        actionsMenu.AllCarts = allCachedCarts;
                    }

                    if (authenticatedInstitution.IsRittenhouseAdmin())
                    {
                        var resellers = _cartServiceFactory().GetResellers();
                        if (resellers.Any())
                        {
                            actionsMenu.ResellerCartList = new List<SelectListItem>
                            {
                                new SelectListItem
                                {
                                    Selected = true,
                                    Value = "",
                                    Text = ""
                                }
                            };
                            foreach (var reseller in resellers)
                            {
                                actionsMenu.ResellerCartList.Add(new SelectListItem
                                {
                                    Value = reseller.Id.ToString(),
                                    Text = reseller.DisplayName
                                });
                            }

                            actionsMenu.ResellerLinkHref = urlHelper.Action("CreateResellerCart", "Cart",
                                new { institutionId = adminInstitution.Id });
                        }
                    }
                }
            }

            adminBaseModel.ActionsMenu = actionsMenu;
        }
    }
}