#region

using System;
using R2V2.Infrastructure.Storages;

#endregion

namespace R2V2.Core.Resource
{
    public class CachedDiscountFactory
    {
        //public const string ActiveDiscountKey = "Active.Discounts";
        public const string ActiveDiscountTimestampKey = "Active.Discounts.TimeStamp";
        private readonly IApplicationWideStorageService _applicationWideStorageService;

        public CachedDiscountFactory(IApplicationWideStorageService applicationWideStorageService)
        {
            _applicationWideStorageService = applicationWideStorageService;
        }

        public bool HaveDiscountsChanged(DateTime dateCached)
        {
            var timeStamp = GetDiscountTimeStamp();
            if (timeStamp > dateCached)
            {
                return true;
            }

            return false;
        }

        public DateTime GetDiscountTimeStamp()
        {
            if (_applicationWideStorageService.Has(ActiveDiscountTimestampKey))
            {
                return _applicationWideStorageService.Get<DateTime>(ActiveDiscountTimestampKey);
            }

            return DateTime.MinValue.AddMinutes(1);
        }

        public void ResetDiscountTimeStamp()
        {
            if (_applicationWideStorageService.Has(ActiveDiscountTimestampKey))
            {
                _applicationWideStorageService.Remove(ActiveDiscountTimestampKey);
            }

            _applicationWideStorageService.Put(ActiveDiscountTimestampKey, DateTime.Now);
        }
    }
}