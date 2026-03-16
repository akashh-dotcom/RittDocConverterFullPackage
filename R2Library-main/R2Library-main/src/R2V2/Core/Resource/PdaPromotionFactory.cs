#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Core.CollectionManagement;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Resource
{
    public class PdaPromotionFactory
    {
        public const string ActivePdaPromotions = "Active.PdaPromotions";
        public const string ActivePdaPromotionsTimestamp = "Active.PdaPromotions.Timestamp";
        private readonly IApplicationWideStorageService _applicationWideStorageService;
        private readonly CachedDiscountFactory _cachedDiscountFactory;

        private readonly ILog<PdaPromotionFactory> _log;
        private readonly IQueryable<PdaPromotion> _pdaPromotions;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public PdaPromotionFactory(
            ILog<PdaPromotionFactory> log
            , IApplicationWideStorageService applicationWideStorageService
            , IQueryable<PdaPromotion> pdaPromotions
            , IUnitOfWorkProvider unitOfWorkProvider
            , CachedDiscountFactory cachedDiscountFactory)
        {
            _log = log;
            _applicationWideStorageService = applicationWideStorageService;
            _pdaPromotions = pdaPromotions;
            _unitOfWorkProvider = unitOfWorkProvider;
            _cachedDiscountFactory = cachedDiscountFactory;
        }

        public IEnumerable<CachedPdaPromotion> GetCachedPdaPromotions(bool forceReload = false)
        {
            var cachedPdaPromotions = _applicationWideStorageService.Get<List<CachedPdaPromotion>>(ActivePdaPromotions);

            var timeStamp = DateTime.MinValue;

            if (_applicationWideStorageService.Has(ActivePdaPromotionsTimestamp))
            {
                timeStamp = _applicationWideStorageService.Get<DateTime>(ActivePdaPromotionsTimestamp);
            }

            if (forceReload ||
                (cachedPdaPromotions == null && timeStamp == DateTime.MinValue) ||
                (timeStamp != DateTime.MinValue && _cachedDiscountFactory.HaveDiscountsChanged(timeStamp)))
            {
                _log.Debug("Gettting NEW PdaPromotions");
                var pdaPromotions = GetDbPdaPromotions().ToList();

                if (pdaPromotions.Any())
                {
                    cachedPdaPromotions = pdaPromotions.Select(x => new CachedPdaPromotion(x)).ToList();
                    _applicationWideStorageService.Put(ActivePdaPromotions, cachedPdaPromotions);
                    _applicationWideStorageService.Put(ActivePdaPromotionsTimestamp, DateTime.Now);
                }
                else
                {
                    _applicationWideStorageService.Put(ActivePdaPromotionsTimestamp, DateTime.Now);
                }
            }

            return cachedPdaPromotions;
        }

        public IEnumerable<PdaPromotion> GetDbPdaPromotions()
        {
            return _pdaPromotions.OrderByDescending(x => x.StartDate);
        }

        private void RemovePdaPromotionsFromCache()
        {
            _applicationWideStorageService.Remove(ActivePdaPromotions);
            _applicationWideStorageService.Remove(ActivePdaPromotionsTimestamp);
            _cachedDiscountFactory.ResetDiscountTimeStamp();
        }

        public bool DeletePdaPromotion(int pdaPromotionId)
        {
            _log.DebugFormat("DeletePdaPromotion(pdaPromotionId: {0})", pdaPromotionId);
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var pdaPromotion = _pdaPromotions.FirstOrDefault(x => x.Id == pdaPromotionId);

                    if (pdaPromotion != null)
                    {
                        uow.Delete(pdaPromotion);
                        uow.Commit();
                        transaction.Commit();
                        RemovePdaPromotionsFromCache();
                        return true;
                    }

                    transaction.Rollback();
                    return false;
                }
            }
        }

        public bool SavePdaPromotion(PdaPromotion pdaPromotion)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var dbPdaPromotion = _pdaPromotions.FirstOrDefault(x => x.Id == pdaPromotion.Id) ??
                                             new PdaPromotion { RecordStatus = true };

                        dbPdaPromotion.Name = pdaPromotion.Name;
                        dbPdaPromotion.Discount = pdaPromotion.Discount;
                        dbPdaPromotion.Description = pdaPromotion.Description;
                        dbPdaPromotion.StartDate = pdaPromotion.StartDate;

                        dbPdaPromotion.EndDate =
                            pdaPromotion.EndDate.AddDays(1).AddMinutes(-1); //Will make it 11L59pm on the day stated. 

                        if (DoesPdaPromotionOverLap(dbPdaPromotion))
                        {
                            return false;
                        }

                        uow.SaveOrUpdate(dbPdaPromotion);

                        uow.Commit();

                        transaction.Commit();
                        RemovePdaPromotionsFromCache();
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

        public bool DoesPdaPromotionOverLap(PdaPromotion pdaPromotion)
        {
            var foundPdaPromotions = _pdaPromotions.Where(x => x.Id != pdaPromotion.Id &&
                                                               ((x.StartDate >= pdaPromotion.StartDate &&
                                                                 x.StartDate <= pdaPromotion.EndDate) ||
                                                                (x.EndDate >= pdaPromotion.StartDate &&
                                                                 x.EndDate <= pdaPromotion.EndDate)
                                                               ));


            return foundPdaPromotions.Any();
        }
    }
}