#region

using R2V2.Core.Search;

#endregion

namespace R2V2.Web.Models.Search.Fields
{
    public class BookTitleField : SearchFieldBase
    {
        public BookTitleField()
            : base("book-title", SearchFields.BookTitle, SearchType.FrontMatter)
        {
        }
    }
}