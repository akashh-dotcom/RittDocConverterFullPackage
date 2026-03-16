namespace R2V2.DataAccess.DtSearch
{
    public interface ISearchInitializer
    {
        string DtSearchVersion { get; }

        bool Init();
    }
}