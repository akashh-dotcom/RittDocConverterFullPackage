namespace R2V2.Core.Tabers
{
    public interface ITermContent
    {
        int Id { get; set; }
        string Term { get; set; }
        string Content { get; set; }
        string SectionId { get; set; }
    }
}