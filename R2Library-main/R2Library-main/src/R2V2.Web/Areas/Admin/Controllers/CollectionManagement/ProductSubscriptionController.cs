#region

using System.Linq;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using ProductSubscription = R2V2.Core.CollectionManagement.ProductSubscription;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers.CollectionManagement
{
    [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN })]
    public class ProductSubscriptionController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly ICollectionManagementSettings _collectionManagementSettings;
        private readonly InstitutionService _institutionService;
        private readonly ILog<ProductSubscriptionController> _log;
        private readonly IQueryable<Product> _products;
        private readonly IQueryable<ProductSubscription> _productSubscriptions;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public ProductSubscriptionController(ILog<ProductSubscriptionController> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IAuthenticationContext authenticationContext
            , IAdminContext adminContext
            , IQueryable<Product> products
            , IQueryable<ProductSubscription> productSubscriptions
            , ICollectionManagementSettings collectionManagementSettings
            , InstitutionService institutionService
        )
            : base(authenticationContext)
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _adminContext = adminContext;
            _products = products;
            _productSubscriptions = productSubscriptions;
            _collectionManagementSettings = collectionManagementSettings;
            _institutionService = institutionService;
        }
    }
}