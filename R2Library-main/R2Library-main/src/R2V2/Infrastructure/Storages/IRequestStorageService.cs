namespace R2V2.Infrastructure.Storages
{
    public interface IRequestStorageService : IStorageService
    {
        object this[string key] { get; set; }
    }
}