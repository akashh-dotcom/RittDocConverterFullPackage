namespace R2V2.Infrastructure.Storages
{
    public interface IUserSessionStorageService : IStorageService
    {
        int Timeout { get; }
        void ClearAll();
    }
}