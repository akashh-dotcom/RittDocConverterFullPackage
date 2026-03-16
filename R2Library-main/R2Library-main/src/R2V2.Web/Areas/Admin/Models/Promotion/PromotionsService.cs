#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Promotion
{
    public class PromotionsService
    {
        private readonly ILog<PromotionsService> _log;
        private readonly IQueryable<Product> _products;
        private readonly PromotionsFactory _promotionsFactory;
        private readonly SpecialDiscountResourceFactory _specialDiscountResourceFactory;

        public PromotionsService(ILog<PromotionsService> log
            , IQueryable<Product> products
            , PromotionsFactory promotionsFactory
        )
        {
            _log = log;
            _products = products;
            _promotionsFactory = promotionsFactory;
            //_carts = carts;
        }

        public Promotions GetPromotions()
        {
            var promotions = _promotionsFactory.GetCachedPromotions();
            return new Promotions(promotions, _products);
        }

        public PromotionModel GetPromotion(int promotionId)
        {
            _log.DebugFormat("GetPromotion(promotionId: {0})", promotionId);
            var promotion = _promotionsFactory.GetCachedPromotions().FirstOrDefault(x => x.Id == promotionId);
            return new PromotionModel(promotion, _products);
        }

        public CachedPromotion GetPromotion(string code)
        {
            _log.DebugFormat("GetPromotion(code: {0})", code);
            var promotion = _promotionsFactory.GetCachedPromotions().FirstOrDefault(x => x.Code == code);
            return promotion;
        }

        public Core.CollectionManagement.Promotion GetDbPromotion(string code)
        {
            _log.DebugFormat("GetPromotion(code: {0})", code);
            var promotion = _promotionsFactory.GetPromotions().FirstOrDefault(x => x.Code == code);
            return promotion;
        }

        public bool DeletePromotion(int promotionId)
        {
            return _promotionsFactory.DeletePromotion(promotionId);
        }

        public bool SavePromotion(PromotionModel model)
        {
            var promotion = new Core.CollectionManagement.Promotion
            {
                Code = model.Code.Trim(),
                Name = model.Name,
                Discount = model.Discount,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                OrderSource = "N/A", //model.OrderSource,
                Id = model.PromotionId,
                MaximumUses = model.MaximumUses,
                EnableCartAlert = model.EnableCartAlert,
                PromotionProducts = new List<PromotionProduct>()
            };

            if (model.ProductsSelected != null)
            {
                foreach (var i in model.ProductsSelected)
                {
                    promotion.PromotionProducts.Add(new PromotionProduct
                        { ProductId = i, PromotionId = model.PromotionId });
                }
            }

            return _promotionsFactory.SavePromotion(promotion);
        }
    }
}