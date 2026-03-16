#region

using System;
using System.Collections;
using System.Web;
using System.Web.Caching;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Infrastructure.Storages
{
    public class ApplicationWideStorageService : IApplicationWideStorageService
    {
        private readonly ICacheSettings _cacheSettings;
        private readonly Cache _httpCache;
        private readonly ILog<ApplicationWideStorageService> _log;

        public ApplicationWideStorageService(ILog<ApplicationWideStorageService> log, ICacheSettings cacheSettings)
        {
            _log = log;
            _cacheSettings = cacheSettings;
            _httpCache = HttpRuntime.Cache;
        }

        public T Get<T>(string key)
        {
            return (T)Get(key);
        }

        public object Get(string key)
        {
            var item = (ApplicationStorageItem)_httpCache.Get(key) ?? new ApplicationStorageItem();
            return item.Value;
            //return _httpCache.Get(key);
        }

        public void Put(string key, object value)
        {
            Put(key, value, DateTime.Now.AddHours(_cacheSettings.DefaultExpirationInHours));
        }

        public void Remove(string key)
        {
            _log.DebugFormat("Delete() - key: {0}", key);
            _httpCache.Remove(key);
        }

        public bool Has(string key)
        {
            return Get(key) != null;
        }

        public void Put(string key, object value, DateTime expirationDate)
        {
            _log.InfoFormat("Put() - key: {0}, expirationDate: {1}", key, expirationDate);
            var item = new ApplicationStorageItem
                { Key = key, Value = value, ExpirationDate = expirationDate, InsertDate = DateTime.Now };

            if (_httpCache.Get(key) != null)
            {
                _httpCache.Remove(key);
            }

            _httpCache.Insert(key, item, null, expirationDate, Cache.NoSlidingExpiration, CacheItemPriority.Normal,
                null);
        }

        public int Count()
        {
            return _httpCache.Count;
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return _httpCache.GetEnumerator();
        }
    }
}