#region

using R2V2.Core.Search;

#endregion

namespace R2V2.Web.Models.Search.Fields
{
    public class FullTextField : SearchFieldBase
    {
        public FullTextField()
            : base("full-text", SearchFields.FullText, SearchType.FullText)
        {
        }
    }
}