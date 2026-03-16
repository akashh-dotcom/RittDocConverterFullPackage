#region

using System.Web;
using R2V2.Core;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;

#endregion

namespace R2V2.Web.Infrastructure.Storages
{
    public class UserSessionStorageService : IUserSessionStorageService
    {
        private const int DefaultSessionTimeout = 20;
        private readonly ILog<UserSessionStorageService> _log;

        public UserSessionStorageService(ILog<UserSessionStorageService> log)
        {
            _log = log;
        }

        private HttpSessionStateBase Session =>
            HttpContext.Current != null && HttpContext.Current.Session != null
                ? new HttpSessionStateWrapper(HttpContext.Current.Session)
                : null;

        #region IUserSessionStorage Members

        public T Get<T>(string key)
        {
            return (T)Get(key);
        }

        public object Get(string key)
        {
            if (Session != null)
            {
                return Session[key];
            }

            _log.WarnFormat("Get() - key: {0} - Session is null!", key);
            return null;
        }

        public void Put(string key, object value)
        {
            if (Session == null)
            {
                _log.WarnFormat("Put() - key: {0} - Session is null!", key);
                return;
            }

            _log.DebugFormat("Put() - key: {0}", key);
            Session[key] = value;
        }

        public void Remove(string key)
        {
            if (Session == null)
            {
                _log.WarnFormat("Remove() - key: {0} - Session is null!", key);
                return;
            }

            Session.Remove(key);
            foreach (string k in Session.Keys)
            {
                var o = Session[k];
                var debugInfo = o as IDebugInfo;
                _log.DebugFormat("key: {0} = {1}", k, debugInfo != null ? debugInfo.ToDebugString() : o?.ToString());
            }
        }

        public bool Has(string key)
        {
            return Session?[key] != null;
        }

        public void ClearAll()
        {
            Session?.Clear();
        }

        // todo:  figure out what causes Session to be null sometimes - JAH 6/27/12
        // return Session != null ? Session.Timeout : DefaultSessionTimeout;
        public int Timeout => DefaultSessionTimeout;

        #endregion
    }
}