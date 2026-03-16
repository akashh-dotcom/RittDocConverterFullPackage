#region

using R2V2.Core.Search;

#endregion

namespace R2V2.Web.Models.Search.Fields
{
    public class ChapterTitleField : SearchFieldBase
    {
        public ChapterTitleField()
            : base("chapter-title", SearchFields.ChapterTitle, SearchType.FrontMatter)
        {
        }
    }
}