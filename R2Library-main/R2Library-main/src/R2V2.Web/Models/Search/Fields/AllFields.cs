#region

using R2V2.Core.Search;

#endregion

namespace R2V2.Web.Models.Search.Fields
{
    public class AllFields : SearchFieldBase
    {
        public AllFields() : base("", SearchFields.All, SearchType.FullText)
        {
        }
    }
}