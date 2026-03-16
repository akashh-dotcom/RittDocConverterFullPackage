#region

using R2V2.Core.Search;

#endregion

namespace R2V2.Web.Models.Search.Fields
{
    public class SectionTitleField : SearchFieldBase
    {
        public SectionTitleField()
            : base("section-title", SearchFields.SectionTitle, SearchType.FrontMatter)
        {
        }
    }
}