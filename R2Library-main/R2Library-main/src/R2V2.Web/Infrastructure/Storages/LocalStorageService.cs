#region

using System;
using System.Collections;
using System.Linq;
using System.Web;
using R2V2.Extensions;
using R2V2.Infrastructure.Storages;

#endregion

namespace R2V2.Web.Infrastructure.Storages
{
    public class LocalStorageService : ILocalStorageService
    {
        //private static readonly object LocalDataKey = new object();
        private static readonly string LocalDataKey = "LocalStorageService.Hashtable";

        protected Hashtable Local
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    if (!HttpContext.Current.Items.Contains(LocalDataKey))
                    {
                        var hashTable = new Hashtable();
                        HttpContext.Current.Items[LocalDataKey] = hashTable;

                        return hashTable;
                    }

                    return HttpContext.Current.Items[LocalDataKey] as Hashtable;
                }

                return null;
            }
        }

        #region ILocalStorage Members

        public void Remove(string key)
        {
            if (!Local.ContainsKey(key))
            {
                return;
            }

            var item = Local[key];
            if (item is IDisposable)
            {
                item.As<IDisposable>().Dispose();
            }

            Local.Remove(key);
        }

        public void Clear()
        {
            foreach (var item in Local.Values.OfType<IDisposable>())
            {
                item.As<IDisposable>().Dispose();
            }

            Local.Clear();
        }

        public T Get<T>(object key) where T : class
        {
            var item = Get(key);

            return item != null ? item.As<T>() : null;
        }

        public object Get(object key)
        {
            var item = Local[key];
            if (item is WeakReference)
            {
                var reference = item.As<WeakReference>();
                return reference.IsAlive ? reference.Target : null;
            }

            return item;
        }

        public void PutWeak(object key, object item)
        {
            Put(key, new WeakReference(item));
        }

        public void Put(object key, object item)
        {
            Local[key] = item;
        }

        public bool Has(object key)
        {
            bool has;
            var item = Local[key];

            if (item is WeakReference)
            {
                has = item.As<WeakReference>().IsAlive;
                return has;
            }

            has = item != null;
            return has;
        }

        public void Dispose()
        {
            var tempList = new ArrayList(Local.Values);

            foreach (var item in tempList)
            {
                if (item is IDisposable)
                {
                    item.As<IDisposable>().Dispose();
                }

                if (!(item is WeakReference))
                {
                    continue;
                }

                var weakItem = item.As<WeakReference>();
                if (weakItem.IsAlive && weakItem.Target is IDisposable)
                {
                    weakItem.Target.As<IDisposable>().Dispose();
                }
            }
        }

        public ICollection Keys()
        {
            return Local.Keys;
        }

        #endregion
    }
}