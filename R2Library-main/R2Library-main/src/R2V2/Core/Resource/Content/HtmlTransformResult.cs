namespace R2V2.Core.Resource.Content
{
    public class HtmlTransformResult : ITransformResult
    {
        public string Result { get; set; }
        public long TransformTime { get; set; }

        public string Isbn { get; set; }
        public string Section { get; set; }
        public string OutputFilename { get; set; }
    }
}