namespace R2V2.Core.Resource.Collection
{
    public interface ICollection : IDebugInfo
    {
        int Id { get; set; }
        string Name { get; set; }
        bool HideInFilter { get; set; }
        int Sequence { get; set; }
        bool IsSpecialCollection { get; set; }
        int SpecialCollectionSequence { get; set; }
        string Description { get; set; }
        bool IsPublic { get; set; }
    }
}