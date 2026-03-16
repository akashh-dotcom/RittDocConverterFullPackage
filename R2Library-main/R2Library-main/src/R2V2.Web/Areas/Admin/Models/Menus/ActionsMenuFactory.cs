#region

using System.Web.Mvc;
using R2V2.Core.Institution;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Core.Territory;
using R2V2.Web.Areas.Admin.Controllers;
using R2V2.Web.Areas.Admin.Controllers.CollectionManagement;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;
using R2V2.Web.Areas.Admin.Models.Institution;
using R2V2.Web.Areas.Admin.Models.ReserveShelfManagement;
using R2V2.Web.Areas.Admin.Models.Resource;
using R2V2.Web.Areas.Admin.Models.Review;
using R2V2.Web.Areas.Admin.Models.User;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Menus
{
    public static class ActionsMenuBuilderFactory
    {
        public static IActionsMenuBuilder CreateActionsMenuBuilder(ControllerBase controller,
            IPracticeAreaService practiceAreaService, ISpecialtyService specialtyService,
            ICollectionService collectionService, ITerritoryService territoryService, IWebSettings webSettings,
            ICollectionService collecitonService, IInstitutionTypeService institutionTypeService)
        {
            if (controller is CollectionManagementController)
            {
                return new CollectionManagementActionsMenuBuilder(practiceAreaService, specialtyService,
                    collecitonService);
            }

            if (controller is ReserveShelfManagementController)
            {
                return new ReserverShelfManagementActionsMenuBuilder(practiceAreaService, specialtyService,
                    collectionService);
            }

            if (controller is ReviewController)
            {
                return new ReviewActionsMenuBuilder(practiceAreaService, specialtyService, collectionService);
            }

            if (controller is InstitutionController)
            {
                return new InstitutionActionsMenuBuilder(territoryService, institutionTypeService);
            }

            if (controller is UserController)
            {
                return new UserActionsMenuBuilder();
            }

            if (controller is ReportController ||
                controller is IpAddressRangeController ||
                controller is InstitutionReferrerController ||
                controller is NotesController ||
                controller is OrderHistoryController ||
                controller is InstitutionBrandingController ||
                controller is PublisherController ||
                controller is CounterReportController ||
                controller is CartController ||
                controller is MarketingController
               )
            {
                return new GenericInstitutionActionsMenuBuilder();
            }

            return new ResourceActionsMenuBuilder(practiceAreaService, specialtyService, collectionService,
                webSettings);
        }
    }
}