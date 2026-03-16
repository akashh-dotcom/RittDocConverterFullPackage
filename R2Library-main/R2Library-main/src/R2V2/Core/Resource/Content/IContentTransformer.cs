namespace R2V2.Core.Resource.Content
{
    public interface IContentTransformer
    {
        string Isbn { get; set; }
        string Section { get; set; }

        ITransformResult Transform(ContentType contentType, ResourceAccess resourceAccess, string baseUrl, bool email);
    }
}