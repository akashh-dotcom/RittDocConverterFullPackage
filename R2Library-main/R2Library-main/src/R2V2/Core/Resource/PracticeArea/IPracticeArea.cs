namespace R2V2.Core.Resource.PracticeArea
{
    public interface IPracticeArea : IDebugInfo
    {
        int Id { get; set; }
        string Code { get; set; }
        string Name { get; set; }
        int SequenceNumber { get; set; }
    }
}