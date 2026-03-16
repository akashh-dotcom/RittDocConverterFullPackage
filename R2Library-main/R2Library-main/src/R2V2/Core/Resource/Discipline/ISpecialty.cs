namespace R2V2.Core.Resource.Discipline
{
    public interface ISpecialty : IDebugInfo
    {
        int Id { get; set; }
        string Name { get; set; }
        string Code { get; set; }
        int SequenceNumber { get; set; }
    }
}