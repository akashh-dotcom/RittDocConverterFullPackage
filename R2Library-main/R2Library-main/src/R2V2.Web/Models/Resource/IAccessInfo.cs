namespace R2V2.Web.Models.Resource
{
    public interface IAccessInfo
    {
        bool IsFullTextAvailable { get; }
        bool IsArchive { get; }
        bool IsForthcoming { get; }

        bool IsPdaResource { get; set; }
    }
}