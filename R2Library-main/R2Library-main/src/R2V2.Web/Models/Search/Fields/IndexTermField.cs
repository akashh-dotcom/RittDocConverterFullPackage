#region

using R2V2.Core.Search;

#endregion

namespace R2V2.Web.Models.Search.Fields
{
    public class IndexTermField : SearchFieldBase
    {
        public IndexTermField()
            : base("index-terms", SearchFields.IndexTerms, SearchType.TopicIndex)
        {
        }
    }
}