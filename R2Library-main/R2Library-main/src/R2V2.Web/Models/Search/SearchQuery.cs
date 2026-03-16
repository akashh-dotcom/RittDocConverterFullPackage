#region

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

#endregion

namespace R2V2.Web.Models.Search
{
    public interface ISearchQuery
    {
        int Page { get; set; }
        int PageSize { get; set; }
        string SortBy { get; set; }
        string Within { get; set; }
        string Disciplines { get; set; }
        string Filter { get; set; }
        string Field { get; set; }
        string PracticeArea { get; set; }
        string Author { get; set; }
        string Title { get; set; }
        string Publisher { get; set; }
        string Editor { get; set; }
        string Isbn { get; set; }
        bool TocAvailable { get; set; }
        int Include { get; set; }
        int FilterYear { get; }
        int MinYear { get; }
        int MaxYear { get; }
        string Year { get; set; }
        AuthenticationInfo AuthenticationInfo { get; set; }
        Layout Layout { get; set; }
        Header Header { get; set; }
        Footer Footer { get; set; }
        SortedList<int, HeaderTab> Tabs { get; set; }
        string Q { get; set; }
        AdvancedSearchModel AdvancedSearch { get; set; }
    }

    public class SearchQuery : BaseModel, ISearchQuery
    {
        private int _filterYear;
        private int _maxYear;
        private int _minYear;
        private string _year;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; }
        public string Within { get; set; }
        public string Disciplines { get; set; }
        public string Filter { get; set; }
        public string Field { get; set; } = "";
        public string PracticeArea { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Publisher { get; set; }
        public string Editor { get; set; }
        public string Isbn { get; set; }
        public bool TocAvailable { get; set; }
        public int Include { get; set; } = 1;
        public int FilterYear => _filterYear;
        public int MinYear => _minYear;
        public int MaxYear => _maxYear;

        public string Year
        {
            get => _year;
            set
            {
                _year = value;
                if (!string.IsNullOrWhiteSpace(_year))
                {
                    if (_year.Length == 4)
                    {
                        int.TryParse(_year, out _filterYear);
                    }
                    else if (_year.IndexOf("-", StringComparison.Ordinal) == 4)
                    {
                        int.TryParse(_year.Substring(0, 4), out _minYear);
                        int.TryParse(_year.Substring(5), out _maxYear);
                        if (_minYear < _maxYear)
                        {
                            var min = _maxYear;
                            _maxYear = _minYear;
                            _minYear = min;
                        }
                    }
                }
            }
        }


        public string GetText()
        {
            var query = new StringBuilder();
            if (!string.IsNullOrEmpty(Q))
            {
                query.Append(Q);
            }

            AppendFieldToQuery(query, "author", Author);
            AppendFieldToQuery(query, "title", Title);
            AppendFieldToQuery(query, "publisher", Publisher);
            AppendFieldToQuery(query, "editor", Editor);
            AppendFieldToQuery(query, "year", Year);
            AppendFieldToQuery(query, "isbn", Isbn);
            return query.ToString();
        }

        public string GetDescription()
        {
            var query = new StringBuilder();
            if (!string.IsNullOrEmpty(Q))
            {
                query.Append(Q);
            }

            AppendFieldToQuery(query, "author", Author);
            AppendFieldToQuery(query, "title", Title);
            AppendFieldToQuery(query, "publisher", Publisher);
            AppendFieldToQuery(query, "editor", Editor);
            AppendFieldToQuery(query, "year", Year);
            if (MinYear > 0 && MaxYear > 0)
            {
                AppendFieldToQuery(query, "years", $"{MinYear}-{MaxYear}");
            }

            AppendFieldToQuery(query, "isbn", Isbn);
            AppendFieldToQuery(query, "results from", Field);
            AppendFieldToQuery(query, "filter", Filter);
            AppendFieldToQuery(query, "practice area", PracticeArea);
            AppendFieldToQuery(query, "disciplines", Disciplines);
            return query.ToString();
        }

        /// <summary>
        /// </summary>
        private void AppendFieldToQuery(StringBuilder query, string fieldName, string fieldValue)
        {
            if (!string.IsNullOrEmpty(fieldValue))
            {
                query.AppendFormat("{0}{1}:{2}", query.Length > 0 ? "; " : string.Empty, fieldName, fieldValue);
            }
        }

        public string GetDescriptionListForDisplay()
        {
            var sb = new StringBuilder();

            AppendFieldToListForDisplay(sb, "Search Term", Q);

            if (Include > 2)
            {
                if (TocAvailable)
                {
                    AppendFieldToListForDisplay(sb, "Include", "Active, Archived, & Table of Contents");
                }

                AppendFieldToListForDisplay(sb, "Include", "Active & Archived");
            }
            else if (Include == 2)
            {
                if (TocAvailable)
                {
                    AppendFieldToListForDisplay(sb, "Include", "Archived & Table of Contents");
                }

                AppendFieldToListForDisplay(sb, "Include", "Archived");
            }
            else
            {
                if (TocAvailable)
                {
                    AppendFieldToListForDisplay(sb, "Include", "Active & Table of Contents");
                }

                AppendFieldToListForDisplay(sb, "Include", "Active");
            }

            AppendFieldToListForDisplay(sb, "Author", Author);
            AppendFieldToListForDisplay(sb, "Title", Title);
            AppendFieldToListForDisplay(sb, "Publisher", Publisher);
            AppendFieldToListForDisplay(sb, "Editor", Editor);
            AppendFieldToListForDisplay(sb, "Year", Year);
            if (MinYear > 0 && MaxYear > 0)
            {
                AppendFieldToListForDisplay(sb, "Years", $"{MinYear}-{MaxYear}");
            }

            AppendFieldToListForDisplay(sb, "Isbn", Isbn);
            AppendFieldToListForDisplay(sb, "Results from", Field);
            AppendFieldToListForDisplay(sb, "Filter", Filter);
            return sb.ToString();
        }

        private void AppendFieldToListForDisplay(StringBuilder sb, string fieldName, string fieldValue)
        {
            if (!string.IsNullOrEmpty(fieldName) && !string.IsNullOrEmpty(fieldValue))
            {
                sb.AppendFormat("<li><p class=\"search-terms\"><strong>{0}</strong>: {1}</p></li>", fieldName,
                    fieldValue);
            }
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder("SearchQuery = [");
            sb.AppendFormat("Q: {0}", Q);
            sb.AppendFormat(", Page: {0}", Page);
            sb.AppendFormat(", PageSize: {0}", PageSize);
            sb.AppendFormat(", Include: {0}", Include);
            sb.AppendFormat(", TocAvailable: {0}", TocAvailable);
            sb.AppendFormat(", SortBy: {0}", SortBy);
            sb.AppendFormat(", PracticeArea: {0}", PracticeArea);
            sb.AppendFormat(", Field: {0}", Field);
            sb.AppendFormat(", Filter: {0}", Filter);
            sb.AppendFormat(", Disciplines: {0}", Disciplines);
            sb.AppendFormat(", Author: {0}", Author);
            sb.AppendFormat(", Title: {0}", Title);
            sb.AppendFormat(", Publisher: {0}", Publisher);
            sb.AppendFormat(", Editor: {0}", Editor);
            sb.AppendFormat(", Within: {0}", Within);

            sb.AppendFormat(", Isbn: {0}", Isbn);

            sb.Append("]");
            return sb.ToString();
        }

        public RouteValueDictionary ToRouteValues(bool isJson)
        {
            if (isJson)
            {
                return new RouteValueDictionary
                {
                    { "Q", Q },
                    { "Include", Include },
                    { "TocAvailable", TocAvailable },
                    { "SortBy", SortBy },
                    { "Field", Field },
                    { "Filter", Filter },
                    { "PracticeArea", PracticeArea },
                    { "Disciplines", Disciplines },
                    { "Page", Page },
                    { "PageSize", PageSize },
                    { "Author", Author },
                    { "Title", Title },
                    { "Publisher", Publisher },
                    { "Editor", Editor },
                    { "Isbn", Isbn },
                    { "Within", Within },
                    { "Year", Year }
                };
            }

            return new RouteValueDictionary
            {
                { "q", Q },
                { "include", Include },
                { "toc-available", TocAvailable },
                { "sort-by", SortBy },
                { "field", Field },
                { "filter-by", Filter },
                { "practice-area", PracticeArea },
                { "disciplines", Disciplines },
                { "page", Page },
                { "results-per-page", PageSize },
                { "author", Author },
                { "title", Title },
                { "publisher", Publisher },
                { "editor", Editor },
                { "isbn", Isbn },
                { "within", Within },
                { "year", Year }
            };
        }

        public string GetSearchUrl(string root, UrlHelper urlHelper)
        {
            // todo: need to find a better way, but I need to get this out - SJS - 8/10/12
            var url = new StringBuilder(root);

            if (!string.IsNullOrWhiteSpace(Q))
            {
                url.AppendFormat("?q={0}", Q);
            }

            url.AppendFormat("#include={0}", Include);

            AppendParamater(url, "author", Author, urlHelper);
            AppendParamater(url, "title", Title, urlHelper);
            AppendParamater(url, "publisher", Publisher, urlHelper);
            AppendParamater(url, "editor", Editor, urlHelper);

            AppendParamater(url, "toc-available", TocAvailable ? "true" : null, urlHelper);
            AppendParamater(url, "sort-by", SortBy, urlHelper);
            AppendParamater(url, "field", Field, urlHelper);
            AppendParamater(url, "filter-by", Filter, urlHelper);
            AppendParamater(url, "practice-area", PracticeArea, urlHelper);

            AppendParamater(url, "disciplines", Disciplines, urlHelper);
            AppendParamater(url, "page", Page.ToString(), urlHelper);
            AppendParamater(url, "results-per-page", PageSize > 10 ? PageSize.ToString() : null, urlHelper);
            AppendParamater(url, "isbn", Isbn, urlHelper);
            AppendParamater(url, "within", Within, urlHelper);
            AppendParamater(url, "year", Year, urlHelper);

            return url.ToString();
        }

        private static void AppendParamater(StringBuilder url, string parameterName, string parameterValue,
            UrlHelper urlHelper)
        {
            if (!string.IsNullOrWhiteSpace(parameterValue))
            {
                url.AppendFormat("&{0}={1}", parameterName, urlHelper.Encode(parameterValue));
            }
        }
    }
}