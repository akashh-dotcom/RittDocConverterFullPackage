#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Resource
{
    public class SpecialDiscountResourceFactory
    {
        public const string ActiveSpecialResourceDiscountKey = "Active.SpecialDiscountResources";
        public const string ActiveSpecialResourceDiscountDictionaryKey = "Active.SpecialDiscountResources.Dictionary";
        public const string ActiveSpecialResourceDiscountTimestampKey = "Active.SpecialDiscountResources.TimeStamp";

        public const string ActiveSpecialResourceDiscountDictionaryTimestampKey =
            "Active.SpecialDiscountResources.Dictionary.TimeStamp";

        private readonly IApplicationWideStorageService _applicationWideStorageService;
        private readonly CachedDiscountFactory _cachedDiscountFactory;

        private readonly ILog<SpecialDiscountResourceFactory> _log;
        private readonly IQueryable<SpecialDiscount> _specialDiscounts;
        private readonly IQueryable<SpecialResource> _specialResources;
        private readonly IQueryable<Special> _specials;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public SpecialDiscountResourceFactory(
            ILog<SpecialDiscountResourceFactory> log
            , IQueryable<Special> specials
            , IQueryable<SpecialDiscount> specialDiscounts
            , IQueryable<SpecialResource> specialResources
            , IUnitOfWorkProvider unitOfWorkProvider
            , IApplicationWideStorageService applicationWideStorageService
            , CachedDiscountFactory cachedDiscountFactory
        )
        {
            _log = log;
            _specials = specials;
            _specialDiscounts = specialDiscounts;
            _specialResources = specialResources;
            _unitOfWorkProvider = unitOfWorkProvider;
            _applicationWideStorageService = applicationWideStorageService;
            _cachedDiscountFactory = cachedDiscountFactory;
        }

        public bool SaveSpecial(Special special)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        uow.SaveOrUpdate(special);

                        uow.Commit();
                        transaction.Commit();

                        RemoveSpecialResourceDiscountsFromCache();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }

            return false;
        }

        public int SaveSpecialAndGetId(Special special)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        uow.SaveOrUpdate(special);

                        uow.Commit();
                        transaction.Commit();

                        RemoveSpecialResourceDiscountsFromCache();
                        return special.Id;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }

            return 0;
        }

        public bool DeleteSpecial(Special special)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        uow.Delete(special);

                        uow.Commit();
                        transaction.Commit();

                        RemoveSpecialResourceDiscountsFromCache();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }

            return false;
        }

        public bool SaveSpecialDiscount(SpecialDiscount specialDiscount)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        uow.SaveOrUpdate(specialDiscount);

                        uow.Commit();
                        transaction.Commit();

                        RemoveSpecialResourceDiscountsFromCache();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }

            return false;
        }

        public bool DeleteSpecialDiscount(SpecialDiscount specialDiscount)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        uow.Delete(specialDiscount);

                        uow.Commit();
                        transaction.Commit();

                        RemoveSpecialResourceDiscountsFromCache();

                        return true;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }

            return false;
        }

        public bool InsertSpecialResources(int[] resourceIds, int specialDiscountId, IUser user)
        {
            var sql =
                $@"Insert into tSpecialResource (iSpecialDiscountId, iResourceId, vchCreatorId, dtCreationDate, tiRecordStatus)
Values({specialDiscountId}, @ResourceId, 'user Id: {user.Id} [{user.FirstName}]', getdate(), 1);";
            var insertCount = 0;
            var sqlBuilder = new StringBuilder();

            foreach (var resourceId in resourceIds)
            {
                sqlBuilder.AppendFormat(sql.Replace("@ResourceId", resourceId.ToString()));
                insertCount++;
                if (insertCount >= 100)
                {
                    var success = RunSqlQuery(sqlBuilder.ToString());
                    if (!success)
                    {
                        return false;
                    }

                    sqlBuilder = new StringBuilder();
                    insertCount = 0;
                }
            }

            if (sqlBuilder.Length > 0)
            {
                var success = RunSqlQuery(sqlBuilder.ToString());
                if (!success)
                {
                    return false;
                }
            }

            return true;
        }

        private bool RunSqlQuery(string sqlQuery)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    var query = uow.Session.CreateSQLQuery(sqlQuery);

                    var results = query.List();

                    return true;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }

        public bool SaveSpecialsResource(List<SpecialResource> specialsResource)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        foreach (var specialResource in specialsResource)
                        {
                            uow.SaveOrUpdate(specialResource);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                        return false;
                    }

                    uow.Commit();
                    transaction.Commit();

                    RemoveSpecialResourceDiscountsFromCache();
                }
            }

            return true;
        }

        public bool DeleteSpecialsResource(List<SpecialResource> specialsResource)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        foreach (var specialResource in specialsResource)
                        {
                            uow.Delete(specialResource);
                        }

                        uow.Commit();
                        transaction.Commit();

                        RemoveSpecialResourceDiscountsFromCache();
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                        return false;
                    }
                }
            }

            return true;
        }

        public bool DeleteSpecialResource(int specialResourceId)
        {
            var specialResource = _specialResources.FirstOrDefault(x => x.Id == specialResourceId);
            if (specialResource == null)
            {
                return false;
            }

            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        uow.Delete(specialResource);

                        uow.Commit();
                        transaction.Commit();

                        RemoveSpecialResourceDiscountsFromCache();
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                        return false;
                    }
                }
            }

            return true;
        }

        public List<Special> GetNonExpiredSpecials()
        {
            return _specials.Where(x => DateTime.Now <= x.EndDate && x.RecordStatus).ToList();
        }

        public List<Special> GetAllSpecials()
        {
            return _specials.Where(x => x.RecordStatus).OrderByDescending(x => x.StartDate)
                .ThenByDescending(x => x.EndDate).ToList();
        }

        public Special GetSpecial(int specialId)
        {
            return GetAllSpecials().FirstOrDefault(x => x.Id == specialId);
        }

        public SpecialDiscount GetSpecialDiscount(int specialDiscountId)
        {
            return _specialDiscounts.FirstOrDefault(x => x.Id == specialDiscountId);
        }

        public IEnumerable<SpecialResource> GetSpecialsResource(int resourceId)
        {
            return
                _specialResources.Where(x =>
                    x.ResourceId == resourceId
                    && x.RecordStatus && x.Discount.RecordStatus);
        }

        /// <summary>
        ///     Will Return Only Active Resource Specials
        /// </summary>
        public List<CachedSpecialResource> GetSpecialResourcesDiscount()
        {
            var specialDiscountResourceList = GetSpecialResourcesDiscountForAdminResource();
            var specialDiscountResources = specialDiscountResourceList?.Where(x => DateTime.Now >= x.StartDate);

            return specialDiscountResources?.ToList();
        }

        /// <summary>
        ///     Will Return Resource Specials Active and Future
        /// </summary>
        public List<CachedSpecialResource> GetSpecialResourcesDiscountForAdminResource()
        {
            var specialDiscountResourceList = GetCachedActiveSpecialResourcesDiscount();

            return specialDiscountResourceList?.ToList();
        }

        public List<CachedSpecialResource> GetCachedSpecialResourceForSpecialController()
        {
            IEnumerable<CachedSpecialResource> specialDiscountResourceList =
                _specialResources.Where(x => x.RecordStatus && x.Discount.RecordStatus)
                    .OrderByDescending(x => x.Discount.DiscountPercentage)
                    .Select(x => new CachedSpecialResource(x));


            return specialDiscountResourceList.Any() ? specialDiscountResourceList.ToList() : null;
        }

        public List<CachedSpecialResource> GetCachedSpecialResourceBySpecialId(int specialId)
        {
            var specialDiscountResourceList =
                GetCachedSpecialResourceForSpecialController();
            var specialDiscountResources = specialDiscountResourceList?.Where(x => x.SpecialId == specialId);

            return specialDiscountResources?.ToList();
        }

        public CachedSpecialResource GetCachedSpecialResource(int resourceId)
        {
            var cachedSpecialResourceDictionary = GetCachedActiveSpecialResourcesDiscountDictionary();
            if (cachedSpecialResourceDictionary != null && cachedSpecialResourceDictionary.ContainsKey(resourceId))
            {
                return cachedSpecialResourceDictionary[resourceId];
            }

            return null;
        }


        private IEnumerable<CachedSpecialResource> GetCachedActiveSpecialResourcesDiscount(bool forceReload = false)
        {
            var specialResourcesDiscount =
                _applicationWideStorageService.Get<List<CachedSpecialResource>>(ActiveSpecialResourceDiscountKey);
            var timeStamp = DateTime.MinValue;

            if (_applicationWideStorageService.Has(ActiveSpecialResourceDiscountTimestampKey))
            {
                timeStamp = _applicationWideStorageService.Get<DateTime>(ActiveSpecialResourceDiscountTimestampKey);
            }

            if (forceReload ||
                (specialResourcesDiscount == null && timeStamp == DateTime.MinValue) ||
                (timeStamp != DateTime.MinValue && _cachedDiscountFactory.HaveDiscountsChanged(timeStamp)))
            {
                _log.Debug("Gettting NEW SpecialResourcesDiscount");
                var specialResources = _specialResources.Where(x =>
                        DateTime.Now <= x.Discount.Special.EndDate
                        && x.RecordStatus && x.Discount.RecordStatus)
                    .OrderByDescending(x => x.Discount.DiscountPercentage).ToList();
                if (specialResources.Any())
                {
                    specialResourcesDiscount = specialResources.Select(x => new CachedSpecialResource(x)).ToList();
                    _applicationWideStorageService.Put(ActiveSpecialResourceDiscountKey, specialResourcesDiscount);
                    _applicationWideStorageService.Put(ActiveSpecialResourceDiscountTimestampKey, DateTime.Now);
                }
                else
                {
                    _applicationWideStorageService.Put(ActiveSpecialResourceDiscountTimestampKey, DateTime.Now);
                }
            }

            return specialResourcesDiscount;
        }

        private Dictionary<int, CachedSpecialResource> GetCachedActiveSpecialResourcesDiscountDictionary(
            bool forceReload = false)
        {
            var specialResourcesDictionary =
                _applicationWideStorageService.Get<Dictionary<int, CachedSpecialResource>>(
                    ActiveSpecialResourceDiscountDictionaryKey);
            var timeStamp = DateTime.MinValue;
            if (_applicationWideStorageService.Has(ActiveSpecialResourceDiscountDictionaryTimestampKey))
            {
                timeStamp = _applicationWideStorageService.Get<DateTime>(
                    ActiveSpecialResourceDiscountDictionaryTimestampKey);
            }

            if (forceReload ||
                (specialResourcesDictionary == null && timeStamp == DateTime.MinValue) ||
                (timeStamp != DateTime.MinValue && _cachedDiscountFactory.HaveDiscountsChanged(timeStamp)))
            {
                _log.Debug("Gettting NEW specialResourcesDictionary");
                var specialResources = _specialResources.Where(x =>
                        x.Discount.Special.StartDate <= DateTime.Now && x.Discount.Special.EndDate >= DateTime.Now
                                                                     && x.RecordStatus && x.Discount.RecordStatus)
                    .OrderByDescending(x => x.Discount.DiscountPercentage).ToList();

                var distinctResourceIds = specialResources.Select(x => x.ResourceId).Distinct();

                if (specialResources.Any())
                {
                    specialResourcesDictionary = new Dictionary<int, CachedSpecialResource>();
                    foreach (var distinctResourceId in distinctResourceIds)
                    {
                        var specialDiscountResource =
                            specialResources.FirstOrDefault(x => x.ResourceId == distinctResourceId);
                        specialResourcesDictionary.Add(distinctResourceId,
                            new CachedSpecialResource(specialDiscountResource));
                    }

                    _applicationWideStorageService.Put(ActiveSpecialResourceDiscountDictionaryKey,
                        specialResourcesDictionary);
                    _applicationWideStorageService.Put(ActiveSpecialResourceDiscountDictionaryTimestampKey,
                        DateTime.Now);
                }
                else
                {
                    _applicationWideStorageService.Put(ActiveSpecialResourceDiscountDictionaryTimestampKey,
                        DateTime.Now);
                }
            }

            return specialResourcesDictionary;
        }

        private void RemoveSpecialResourceDiscountsFromCache()
        {
            _applicationWideStorageService.Remove(ActiveSpecialResourceDiscountKey);
            _applicationWideStorageService.Remove(ActiveSpecialResourceDiscountDictionaryKey);

            _cachedDiscountFactory.ResetDiscountTimeStamp();
        }

        public List<SpecialDiscount> GetSpecialDiscounts(int specialId)
        {
            return _specialDiscounts.Where(x => x.RecordStatus && x.SpecialId == specialId).ToList();
        }
    }
}