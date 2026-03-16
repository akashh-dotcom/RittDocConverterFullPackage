#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class PromotionsFactory
    {
        public const string ActivePromotions = "Active.Promotions";
        public const string ActivePromotionsTimestamp = "Active.Promotions.Timestamp";
        private readonly IApplicationWideStorageService _applicationWideStorageService;
        private readonly CachedDiscountFactory _cachedDiscountFactory;

        private readonly ILog<PdaPromotionFactory> _log;
        private readonly IQueryable<Promotion> _promotions;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public PromotionsFactory(ILog<PdaPromotionFactory> log
            , IApplicationWideStorageService applicationWideStorageService
            , IQueryable<Promotion> promotions
            , IUnitOfWorkProvider unitOfWorkProvider
            , CachedDiscountFactory cachedDiscountFactory)
        {
            _log = log;
            _applicationWideStorageService = applicationWideStorageService;
            _promotions = promotions;
            _unitOfWorkProvider = unitOfWorkProvider;
            _cachedDiscountFactory = cachedDiscountFactory;
        }

        public IEnumerable<CachedPromotion> GetCachedPromotions(bool forceReload = false)
        {
            var cachedPromotions = _applicationWideStorageService.Get<List<CachedPromotion>>(ActivePromotions);

            var timeStamp = DateTime.MinValue;

            if (_applicationWideStorageService.Has(ActivePromotionsTimestamp))
            {
                timeStamp = _applicationWideStorageService.Get<DateTime>(ActivePromotionsTimestamp);
            }

            if (forceReload ||
                (cachedPromotions == null && timeStamp == DateTime.MinValue) ||
                (timeStamp != DateTime.MinValue && _cachedDiscountFactory.HaveDiscountsChanged(timeStamp)))
            {
                _log.Debug("Gettting NEW Promotions");
                var promotions = GetPromotions().ToList();

                if (promotions.Any())
                {
                    cachedPromotions = promotions.Select(x => new CachedPromotion(x)).ToList();
                    _applicationWideStorageService.Put(ActivePromotions, cachedPromotions);
                    _applicationWideStorageService.Put(ActivePromotionsTimestamp, DateTime.Now);
                }
                else
                {
                    _applicationWideStorageService.Put(ActivePromotionsTimestamp, DateTime.Now);
                }
            }

            return cachedPromotions;
        }

        public IEnumerable<Promotion> GetPromotions()
        {
            return _promotions.OrderByDescending(x => x.StartDate);
        }

        public bool DeletePromotion(int promotionId)
        {
            _log.DebugFormat("DeletePromotion(promotionId: {0})", promotionId);
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var promotion = _promotions.FirstOrDefault(x => x.Id == promotionId);

                    if (promotion != null)
                    {
                        uow.Delete(promotion);
                        uow.Commit();
                        transaction.Commit();
                        return true;
                    }

                    transaction.Rollback();
                    RemovePromotionsFromCache();
                    return false;
                }
            }
        }

        public bool SavePromotion(Promotion promotion)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var dbPromotion = _promotions.FirstOrDefault(x => x.Id == promotion.Id) ??
                                          new Promotion { RecordStatus = true };

                        dbPromotion.Code = promotion.Code;
                        dbPromotion.Name = promotion.Name;
                        dbPromotion.Discount = promotion.Discount;
                        dbPromotion.Description = promotion.Description;
                        dbPromotion.StartDate = promotion.StartDate;
                        dbPromotion.EndDate =
                            promotion.EndDate.AddDays(1).AddMinutes(-1); //Will make it 11L59pm on the day stated. 
                        dbPromotion.OrderSource = promotion.OrderSource;
                        dbPromotion.MaximumUses = promotion.MaximumUses;
                        dbPromotion.EnableCartAlert = promotion.EnableCartAlert;
                        uow.SaveOrUpdate(dbPromotion);

                        uow.Commit();

                        //A promotion needs to exist before it save the children. 
                        if (dbPromotion.PromotionProducts != null)
                        {
                            dbPromotion.PromotionProducts.Clear();
                        }
                        else if (promotion.PromotionProducts != null)
                        {
                            dbPromotion.PromotionProducts = new List<PromotionProduct>();
                        }

                        if (promotion.PromotionProducts != null && promotion.PromotionProducts.Any() &&
                            dbPromotion.PromotionProducts != null)
                        {
                            foreach (var product in promotion.PromotionProducts)
                            {
                                dbPromotion.PromotionProducts.Add(new PromotionProduct
                                {
                                    ProductId = product.ProductId,
                                    PromotionId = dbPromotion.Id,
                                    RecordStatus = true
                                });
                            }
                        }

                        uow.Merge(dbPromotion);

                        uow.Commit();

                        transaction.Commit();
                        RemovePromotionsFromCache();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        private void RemovePromotionsFromCache()
        {
            _applicationWideStorageService.Remove(ActivePromotions);
            _applicationWideStorageService.Remove(ActivePromotionsTimestamp);
            _cachedDiscountFactory.ResetDiscountTimeStamp();
        }
    }
}