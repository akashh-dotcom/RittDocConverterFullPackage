#region

using System;
using System.Web;
using R2V2.Extensions;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;

#endregion

namespace R2V2.Web.Infrastructure.Storages
{
    public class RequestStorageService : IRequestStorageService
    {
        private readonly HttpContextBase _httpContext;
        private readonly ILog<RequestStorageService> _log;

        public RequestStorageService(HttpContextBase context, ILog<RequestStorageService> log)
        {
            _httpContext = context;
            _log = log;
            //_log.Debug("RequestStorageService() <<<");
        }

        #region IRequestStorage Members

        public T Get<T>(string key)
        {
            return (T)Get(key);
        }

        public object Get(string key)
        {
            return _httpContext.Items[key];
        }

        public void Put(string key, object value)
        {
            //_log.DebugFormat("key: {0}", key);
            _httpContext.Items[key] = value;
        }

        public void Remove(string key)
        {
            _log.DebugFormat("Delete() - key: {0}", key);
            var item = Get(key);
            if (item is IDisposable)
            {
                item.As<IDisposable>().Dispose();
            }

            _httpContext.Items.Remove(key);
        }

        public bool Has(string key)
        {
            return _httpContext.Items.Contains(key);
        }

        public object this[string key]
        {
            get => Get(key);
            set => Put(key, value);
        }

        #endregion
    }
}