#region

using System.Linq;
using R2V2.Contexts;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Export.FileTypes;
using R2V2.Core.OrderHistory;
using R2V2.Core.Reports;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Areas.Admin.Models.OrderHistory;
using R2V2.Web.Areas.Admin.Models.Promotion;
using R2V2.Web.Areas.Admin.Models.Report;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Areas.Admin.Services
{
    public class WebOrderHistoryService : IOrderHistoryService
    {
        private readonly IAdminContext _adminContext;
        private readonly ILog<WebOrderHistoryService> _log;
        private readonly OrderHistoryService _orderHistoryService;
        private readonly PromotionsService _promotionsService;
        private readonly RecommendationsService _recommendationsService;
        private readonly IResourceService _resourceService;
        private readonly IWebImageSettings _webImageSettings;
        private readonly IWebSettings _webSettings;

        public WebOrderHistoryService(
            ILog<WebOrderHistoryService> log
            , IAdminContext adminContext
            , IResourceService resourceService
            , RecommendationsService recommendationsService
            , IWebImageSettings webImageSettings
            , PromotionsService promotionsService
            , OrderHistoryService orderHistoryService
            , IWebSettings webSettings
        )
        {
            _log = log;
            _adminContext = adminContext;
            _resourceService = resourceService;
            _recommendationsService = recommendationsService;
            _webImageSettings = webImageSettings;
            _promotionsService = promotionsService;
            _orderHistoryService = orderHistoryService;
            _webSettings = webSettings;
        }

        public OrderHistoryDetail GetOrderHistoryDetail(int institutionId, int orderHistoryId)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);
            var resources = _resourceService.GetAllResources().ToList();

            var recommendations = _recommendationsService.GetRecommendations(institution.Id);

            var orderHistory =
                _orderHistoryService.GetOrderHistory(institutionId, orderHistoryId, resources, recommendations);

            return new OrderHistoryDetail(institution, orderHistory, _webImageSettings.SpecialIconBaseUrl,
                _webSettings.RequireSsl);
        }

        public OrderHistoryList GetOrderHistoryList(int institutionId)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);
            var orderHistories = _orderHistoryService.GetAllInstitutionOrderHistories(institutionId);
            return new OrderHistoryList(institution, orderHistories);
        }

        public OrderHistoryListExcelExport GetOrderHistoryListExcelExport(int institutionId)
        {
            var orderHistories = _orderHistoryService.GetAllInstitutionOrderHistories(institutionId);
            return new OrderHistoryListExcelExport(orderHistories);
        }

        public OrderHistoryExcelExport GetOrderHistoryDetailExcelExport(int orderHistoryId, string resourceUrl,
            IAdminInstitution adminInstitution)
        {
            var resources = _resourceService.GetAllResources().ToList();

            var recommendations = _recommendationsService.GetRecommendations(adminInstitution.Id);

            var orderHistory =
                _orderHistoryService.GetOrderHistory(adminInstitution.Id, orderHistoryId, resources, recommendations);

            return new OrderHistoryExcelExport(orderHistory, adminInstitution.ProxyPrefix, adminInstitution.UrlSuffix,
                resourceUrl);
        }

        public int GetOrderHistoryId(int cartId, int institutionId)
        {
            return _orderHistoryService.GetOrderHistoryId(cartId, institutionId);
        }

        public OrderHistoryDetail GetOrderHistorySummary(int institutionId, int cartId)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);
            var resources = _resourceService.GetAllResources().ToList();

            var recommendations = _recommendationsService.GetRecommendations(institution.Id);

            var orderHistory = _orderHistoryService.GetOrderHistorySummary(cartId, resources, recommendations);

            return new OrderHistoryDetail(institution, orderHistory);
        }

        public int SaveOrderHistory(CheckoutRequest checkoutRequest, Cart cart, IUnitOfWork uow)
        {
            CachedPromotion promotion = null;
            if (!string.IsNullOrWhiteSpace(cart.PromotionCode))
            {
                promotion = _promotionsService.GetPromotion(cart.PromotionCode);
            }

            var orderHistoryId = _orderHistoryService.SaveOrderHistory(cart, checkoutRequest, promotion, uow);
            return orderHistoryId;
        }

        private ReportRequest ConvertToReportRequest(ReportQuery query)
        {
            return new ReportRequest
            {
                DateRangeEnd = query.DateRangeEnd.GetValueOrDefault(),
                DateRangeStart = query.DateRangeStart.GetValueOrDefault(),
                InstitutionId = query.InstitutionId,
                Period = query.Period,
                Type = ReportType.SalesReport
            };
        }
    }
}