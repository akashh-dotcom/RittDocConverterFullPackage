#region

using System;
using System.Collections;
using System.Linq;
using R2V2.Extensions;
using R2V2.Infrastructure.Storages;

#endregion

namespace R2V2.WindowsService.Infrastructure.Storage
{
    public class LocalStorageService : ILocalStorageService
    {
        private static readonly object LocalDataKey = new object();

        [ThreadStatic] private static Hashtable _threadsLocal = new Hashtable();

        protected Hashtable Local => _threadsLocal ?? (_threadsLocal = new Hashtable());

        #region ILocalStorage Members

        public ICollection Keys()
        {
            return Local.Keys;
        }

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
            //Put(key, new WeakReference(item));
            Put(key, item);
        }

        public void Put(object key, object item)
        {
            Local[key] = item;
        }

        public bool Has(object key)
        {
            var item = Local[key];

            if (item is WeakReference)
            {
                return item.As<WeakReference>().IsAlive;
            }

            return item != null;
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
                    continue;

                var weakItem = item.As<WeakReference>();
                if (weakItem.IsAlive && weakItem.Target is IDisposable)
                {
                    weakItem.Target.As<IDisposable>().Dispose();
                }
            }
        }

        #endregion
    }
}