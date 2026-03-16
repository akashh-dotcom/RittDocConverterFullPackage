#region

using System;
using System.Collections;

#endregion

namespace R2V2.Infrastructure.Storages
{
    public interface ILocalStorageService : IDisposable
    {
        T Get<T>(object key) where T : class;
        object Get(object key);
        void PutWeak(object key, object item);
        void Put(object key, object item);
        bool Has(object key);
        void Clear();
        void Remove(string key);
        ICollection Keys();
    }
}