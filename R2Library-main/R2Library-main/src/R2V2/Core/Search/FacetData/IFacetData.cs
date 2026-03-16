namespace R2V2.Core.Search.FacetData
{
    public interface IFacetData
    {
        int Id { get; set; }
        int Count { get; set; }
        string Name { get; set; }
        string Code { get; set; }
    }
}