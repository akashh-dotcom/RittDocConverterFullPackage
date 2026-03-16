#region

using System;
using System.Linq;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Web.Areas.Admin.Models.PdaPromotion
{
    public class PdaPromotionsService
    {
        private readonly ILog<PdaPromotionsService> _log;
        private readonly PdaPromotionFactory _pdaPromotionFactory;

        public PdaPromotionsService(ILog<PdaPromotionsService> log
            , PdaPromotionFactory pdaPromotionFactory
        )
        {
            _log = log;
            _pdaPromotionFactory = pdaPromotionFactory;
        }

        public PdaPromotions GetPdaPromotions()
        {
            var pdaPromotions = _pdaPromotionFactory.GetCachedPdaPromotions();
            return new PdaPromotions(pdaPromotions);
        }

        public Core.CollectionManagement.PdaPromotion GetActivePdaPromotion()
        {
            var pdaPromotion = _pdaPromotionFactory.GetDbPdaPromotions()
                .FirstOrDefault(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now);
            return pdaPromotion;
        }

        public PdaPromotionModel GetActivePdaPromotionModel()
        {
            var promotion = _pdaPromotionFactory.GetCachedPdaPromotions()
                .FirstOrDefault(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now);
            var pdaPromotion = promotion != null ? new PdaPromotionModel(promotion) : null;
            return pdaPromotion;
        }

        public PdaPromotionModel GetPdaPromotion(int pdaPromotionId)
        {
            _log.DebugFormat("GetPdaPromotion(pdaPromotionId: {0})", pdaPromotionId);
            var promotion = _pdaPromotionFactory.GetCachedPdaPromotions().FirstOrDefault(x => x.Id == pdaPromotionId);
            return new PdaPromotionModel(promotion);
        }

        public bool DeletePdaPromotion(int pdaPromotionId)
        {
            return _pdaPromotionFactory.DeletePdaPromotion(pdaPromotionId);
        }

        public bool SavePdaPromotion(PdaPromotionModel model)
        {
            var pdaPromotion = new Core.CollectionManagement.PdaPromotion
            {
                Name = model.Name,
                Discount = model.Discount,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Id = model.PdaPromotionId
            };

            return _pdaPromotionFactory.SavePdaPromotion(pdaPromotion);
        }
    }
}