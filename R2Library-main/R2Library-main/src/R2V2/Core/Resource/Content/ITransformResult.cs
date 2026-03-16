namespace R2V2.Core.Resource.Content
{
    public interface ITransformResult
    {
        string Isbn { get; set; }
        string Section { get; set; }
        string OutputFilename { get; set; }
    }
}