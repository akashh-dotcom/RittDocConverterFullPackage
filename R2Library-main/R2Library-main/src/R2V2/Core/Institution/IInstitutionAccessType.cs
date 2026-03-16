namespace R2V2.Core.Institution
{
    public interface IInstitutionAccessType
    {
        AccessType Id { get; }
        string Description { get; }
        string LongDescription { get; }
    }
}