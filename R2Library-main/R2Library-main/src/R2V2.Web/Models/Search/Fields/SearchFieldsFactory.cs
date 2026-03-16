#region

using System.Linq;
using R2V2.Core.Search;

#endregion

namespace R2V2.Web.Models.Search.Fields
{
    public class SearchFieldsFactory
    {
        public static ISearchField AllFields = new AllFields();
        public static ISearchField IndexTermField = new IndexTermField();
        public static ISearchField FullTextField = new FullTextField();
        public static ISearchField ChapterTitleField = new ChapterTitleField();
        public static ISearchField SectionTitleField = new SectionTitleField();
        public static ISearchField BookTitleField = new BookTitleField();
        public static ISearchField ImageTitleField = new ImageTitleField();
        public static ISearchField VideoSectionField = new VideoSectionField();

        public static ISearchField[] Fields =
        {
            AllFields, FullTextField, IndexTermField, BookTitleField, ChapterTitleField, SectionTitleField,
            ImageTitleField, VideoSectionField
        };

        public static ISearchField GetSearchFieldByCode(string code)
        {
            return Fields.SingleOrDefault(x => x.Code == code);
        }

        public static ISearchField GetSearchFieldByValue(SearchFields searchField)
        {
            return Fields.SingleOrDefault(x => x.SearchField == searchField);
        }
    }
}