namespace R2V2.Infrastructure.Storages
{
    public interface IStorageService
    {
        T Get<T>(string key);
        object Get(string key);
        void Put(string key, object value);
        void Remove(string key);
        bool Has(string key);
    }
}