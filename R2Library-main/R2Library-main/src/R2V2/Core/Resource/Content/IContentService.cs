namespace R2V2.Core.Resource.Content
{
    public interface IContentService
    {
        ContentItem GetTableOfContents(string isbn, string baseUrl, ResourceAccess resourceAccess, bool email);
        ContentItem GetContent(string isbn, string section, string baseUrl, ResourceAccess resourceAccess, bool email);
    }
}