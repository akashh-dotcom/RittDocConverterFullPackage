#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using R2V2.Core.Resource;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Core.Resource.Topic;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Core.Search
{
    public class SearchRequestBuilder
    {
        private static readonly string RightSingleQuotationMark = $"{Convert.ToChar(0x92)}";
        private static readonly string RightSingleQuotationMarkWithS = $"{Convert.ToChar(0x92)}s";

        private readonly IQueryable<DiseaseSynonym> _diseaseSynonym;
        private readonly IQueryable<DrugSynonym> _drugSynonym;
        private readonly ILog<SearchRequestBuilder> _log;
        private readonly IQueryable<MedicalTermSynonym> _medicalTermSynonym;
        private readonly PracticeAreaService _practiceAreaService;

        private readonly List<string> _searchTerms = new List<string>();
        private readonly List<string> _synonyms = new List<string>();


        public SearchRequestBuilder(ILog<SearchRequestBuilder> log
            , IQueryable<DiseaseSynonym> diseaseSynonym
            , IQueryable<DrugSynonym> drugSynonym
            , IQueryable<MedicalTermSynonym> medicalTermSynonym
            , PracticeAreaService practiceAreaService
        )
        {
            _log = log;
            _diseaseSynonym = diseaseSynonym;
            _drugSynonym = drugSynonym;
            _medicalTermSynonym = medicalTermSynonym;
            _practiceAreaService = practiceAreaService;
        }

        public string GetRequest(ISearchRequest searchRequest, out string searchTermToLog)
        {
            var request = new StringBuilder();
            var logSearchTerm = new StringBuilder();
            if (!string.IsNullOrEmpty(searchRequest.SearchTerm))
            {
                // quick search
                if (IsIsbnValue(searchRequest.SearchTerm))
                {
                    var isbn = searchRequest.SearchTerm.Replace("-", "");
                    request.AppendFormat(
                        "(r2BookSearch/r2isbn10 contains ( {0} )) or (r2BookSearch/r2isbn13 contains ( {0} )) or (r2BookSearch/r2EIsbn contains ( {0} ))",
                        isbn);
                    logSearchTerm.AppendFormat("isbn:{0} ", isbn);
                }
                else
                {
                    logSearchTerm.AppendFormat("{0} ", searchRequest.SearchTerm);
                    _synonyms.AddRange(GetSynonyms(searchRequest.SearchTerm));
                    _searchTerms.Add(searchRequest.SearchTerm);
                    _searchTerms.AddRange(_synonyms);

                    var terms = new StringBuilder();
                    foreach (var term in _searchTerms)
                    {
                        var cleanTerm = CleanSearchTerm(term);
                        if (!string.IsNullOrEmpty(cleanTerm))
                        {
                            terms.AppendFormat("{0}{1}", terms.Length > 0 ? " or " : string.Empty, cleanTerm);
                        }
                    }

                    //terms.AppendFormat(HandleApostrophe(terms.ToString()));

                    switch (searchRequest.Field)
                    {
                        case SearchFields.All:
                        case SearchFields.FullText:
                            request.AppendFormat("( {0} )", terms);
                            break;
                        case SearchFields.IndexTerms:
                            AppendSearchTerm(request, "r2IndexTerms", terms.ToString(), false, logSearchTerm);
                            break;
                        case SearchFields.BookTitle:
                            AppendSearchTerm(request, "r2BookTitle", terms.ToString(), false, logSearchTerm);
                            AppendSearchTerm(request, "r2BookSubTitle", terms.ToString(), false, logSearchTerm);
                            break;
                        case SearchFields.ChapterTitle:
                            AppendSearchTerm(request, "r2ChapterTitle", terms.ToString(), false, logSearchTerm);
                            break;
                        case SearchFields.SectionTitle:
                            AppendSearchTerm(request, "r2SectionTitle", terms.ToString(), false, logSearchTerm);
                            break;
                        case SearchFields.Author:
                            AppendSearchTerm(request, "r2Author", terms.ToString(), false, logSearchTerm);
                            AppendSearchTerm(request, "r2AuthorPrimary", terms.ToString(), false, logSearchTerm);
                            AppendSearchTerm(request, "r2PrimaryAuthor", terms.ToString(), false, logSearchTerm);
                            break;
                        case SearchFields.ImageTitle:
                            AppendSearchTerm(request, "R2imagetitle", terms.ToString(), false, logSearchTerm);
                            break;
                        case SearchFields.VideoSection:
                            AppendSearchTerm(request, "R2videosection", terms.ToString(), false, logSearchTerm);
                            break;
                    }
                }
            }

            // filter - practice area / library
            if (searchRequest.PracticeArea != null)
            {
                var practiceAreas = _practiceAreaService.GetAllPracticeAreas();
                var practiceArea =
                    practiceAreas.Single(x => x.Code.ToLower() == searchRequest.PracticeArea.Code.ToLower());
                if (practiceArea != null)
                {
                    AppendSearchTerm(request,
                        !string.IsNullOrWhiteSpace(searchRequest.SearchTerm)
                            ? "r2PracticeArea"
                            : "r2BookSearch/r2PracticeArea", practiceArea.Code, true, logSearchTerm);
                }
            }

            // advanced search parameters
            AppendAdvancedSearchFields(searchRequest, request, logSearchTerm);

            // ISBN limit
            var searchWithinIsbns =
                searchRequest.SearchWithinIsbns == null || searchRequest.SearchWithinIsbns.Length == 0
                    ? string.Empty
                    : string.Join(" or ", searchRequest.SearchWithinIsbns);
            AppendSearchTerm(request, "Filename", searchWithinIsbns, true, logSearchTerm);

            // drug monograph limit
            if (searchRequest.DrugMonograph)
            {
                AppendSearchTerm(request, "r2DrugMonograph", "DrugMonograph", true, logSearchTerm);
            }

            // year limit
            if (searchRequest.Year > 1900)
            {
                AppendSearchTerm(request, "r2CopyrightYear", $"{searchRequest.Year}", true, logSearchTerm);
            }

            // specialty/discipline
            if (searchRequest.Specialty != null)
            {
                var term = searchRequest.Specialty.Code.Replace(",", " and ");
                AppendSearchTerm(request, "r2Specialty", term, true, logSearchTerm);
            }

            searchTermToLog = logSearchTerm.ToString();

            return request.ToString();
        }

        /// <summary>
        ///     http://support.dtsearch.com/ocr/dtsearch_help.html -- http://support.dtsearch.com/webhelp/dtsearch/daterecog.htm
        /// </summary>
        private void AppendAdvancedSearchFields(ISearchRequest searchRequest, StringBuilder request,
            StringBuilder logSearchTerm)
        {
            var searchSections = !string.IsNullOrWhiteSpace(searchRequest.SearchTerm);

            var termPrefix = searchSections ? "" : "r2BookSearch/";

            // advanced search - author, book title, publisher, editor, isbn
            AppendSearchTerm(request, $"{termPrefix}r2Author", searchRequest.Author, true, logSearchTerm);
            AppendSearchTerm(request, $"{termPrefix}r2Publisher", searchRequest.Publisher, true, logSearchTerm);
            AppendSearchTerm(request, $"{termPrefix}r2AssociatedPublisher", searchRequest.Publisher, false,
                logSearchTerm);
            AppendSearchTerm(request, $"{termPrefix}r2Editor", searchRequest.Editor, true, logSearchTerm);

            if (!string.IsNullOrEmpty(searchRequest.BookTitle))
            {
                var titleSearch = new StringBuilder();
                AppendSearchTerm(titleSearch, $"{termPrefix}r2BookTitle", searchRequest.BookTitle, true,
                    logSearchTerm);
                AppendSearchTerm(titleSearch, $"{termPrefix}r2BookSubTitle", searchRequest.BookTitle, false,
                    logSearchTerm);
                request.AppendFormat("{0}({1})", request.Length > 0 ? " and " : string.Empty, titleSearch);
            }

            var isbns = searchRequest.Isbns == null || searchRequest.Isbns.Length == 0
                ? string.Empty
                : string.Join(" or ", searchRequest.Isbns);
            AppendSearchTerm(request, $"{termPrefix}r2Isbn", isbns, true, logSearchTerm);

            // publication date
            if (searchRequest.PublicationYearMin > 0 && searchRequest.PublicationYearMax > 0)
            {
                // http://support.dtsearch.com/ocr/dtsearch_help.html -- http://support.dtsearch.com/webhelp/dtsearch/daterecog.htm
                var range = $"{searchRequest.PublicationYearMin}~~{searchRequest.PublicationYearMax}";
                AppendSearchTerm(request, $"{termPrefix}r2CopyrightYear", range, true, logSearchTerm);
            }
        }


        public string GetAdminRequest(ISearchRequest searchRequest, out string searchTermToLog)
        {
            var request = new StringBuilder();
            var logSearchTerm = new StringBuilder();
            if (!string.IsNullOrEmpty(searchRequest.SearchTerm))
            {
                var cleanTerm = CleanSearchTerm(searchRequest.SearchTerm);

                // quick search
                if (IsIsbnValue(cleanTerm))
                {
                    var isbn = cleanTerm.Replace("-", "");

                    AppendSearchTerm(request, "r2BookSearch/r2isbn10", cleanTerm, false, logSearchTerm);
                    AppendSearchTerm(request, "r2BookSearch/r2isbn13", cleanTerm, false, logSearchTerm);
                    AppendSearchTerm(request, "r2BookSearch/r2EIsbn", cleanTerm, false, logSearchTerm);

                    logSearchTerm.AppendFormat("isbn:{0} ", isbn);
                }
                else
                {
                    AppendSearchTerm(request, "r2BookSearch/r2BookTitle", cleanTerm, false, logSearchTerm);
                    AppendSearchTerm(request, "r2BookSearch/r2BookSubTitle", cleanTerm, false, logSearchTerm);
                    AppendSearchTerm(request, "r2BookSearch/r2Author", cleanTerm, false, logSearchTerm);
                    AppendSearchTerm(request, "r2BookSearch/r2AuthorPrimary", cleanTerm, false, logSearchTerm);
                    AppendSearchTerm(request, "r2BookSearch/r2PrimaryAuthor", cleanTerm, false, logSearchTerm);
                    AppendSearchTerm(request, "r2BookSearch/r2Publisher", cleanTerm, false, logSearchTerm);
                    AppendSearchTerm(request, "r2BookSearch/r2AssociatedPublisher", cleanTerm, false, logSearchTerm);
                    AppendSearchTerm(request, "r2BookSearch/r2CopyrightYear", cleanTerm, false, logSearchTerm);
                }
            }

            searchTermToLog = logSearchTerm.ToString();

            return $"({request})";
        }


        /// <param name="logging"> </param>
        private void AppendSearchTerm(StringBuilder request, string field, string term, bool isAnd,
            StringBuilder logging)
        {
            var cleanTerm = CleanSearchTerm(term);
            if (!string.IsNullOrEmpty(cleanTerm))
            {
                var booleanOperator = request.Length == 0 ? string.Empty : isAnd ? " and " : " or ";
                request.AppendFormat("{0}({1} contains ( {2} ))", booleanOperator, field, cleanTerm);
                logging.AppendFormat("{0}:{1} ", field, term);
            }
        }

        private bool IsIsbnValue(string value)
        {
            //_log.DebugFormat("value: {0}", value);
            if (string.IsNullOrEmpty(value))
            {
                _log.Debug("value is null/empty");
                return false;
            }

            var tempValue = value.Replace("-", "").Trim().ToLower();

            if (tempValue.Length == 10)
            {
                var numericValue = tempValue.Substring(0, 9);
                var lastDigit = tempValue.Substring(9, 1)[0];

                var isNumeric = (char.IsNumber(lastDigit) || lastDigit == 'x') && long.TryParse(numericValue, out _);
                _log.DebugFormat("ISBN 10, isNumeric: {0}", isNumeric);
                return isNumeric;
            }

            if (tempValue.Length == 13)
            {
                var isNumeric = long.TryParse(tempValue, out _);
                _log.DebugFormat("ISBN 13, isNumeric: {0}", isNumeric);
                return isNumeric;
            }

            _log.Debug($"value is not 10 or 13 characters long, value: {value}");
            return false;
        }

        private IEnumerable<string> GetSynonyms(string term)
        {
            var synonyms = new List<string>();
            try
            {
                var diseaseSynonyms = _diseaseSynonym.Fetch(x => x.Disease)
                    .Where(x => x.Name == term);
                var drugSynonyms = _drugSynonym.Fetch(x => x.Drug)
                    .Where(x => x.Name == term);
                var medicalTermSynonyms = _medicalTermSynonym.Fetch(x => x.MedicalTerm)
                    .Where(x => x.Name == term);

                synonyms.AddRange(diseaseSynonyms.Select(synonym => synonym.Disease.Name));
                synonyms.AddRange(drugSynonyms.Select(synonym => synonym.Drug.Name));
                synonyms.AddRange(medicalTermSynonyms.Select(synonym => synonym.MedicalTerm.Name));
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return synonyms.Distinct().ToList();
        }

        private string CleanSearchTerm(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return string.Empty;
            }

            term = term.Trim();

            //Squish #1195 - search queries containing the word "to" were causing internal dtSearch query errors -DRJ
            if (term.ToLower().Contains(" to ") || term.ToLower().StartsWith("to ") || term.ToLower().EndsWith(" to"))
            {
                if (!term.Contains("\""))
                {
                    term = $"\"{term}\"";
                }
            }

            return term;
        }

        private string HandleApostrophe(string searchTerm)
        {
            var additionalTerms = new StringBuilder();
            if (searchTerm.Contains("'"))
            {
                additionalTerms.AppendFormat(" or ( {0} )", searchTerm.Replace("'", RightSingleQuotationMark));
                additionalTerms.AppendFormat(" or ( {0} )", searchTerm.Replace("'", ""));
                if (searchTerm.Contains("'s"))
                {
                    additionalTerms.AppendFormat(" or ( {0} )", searchTerm.Replace("'s", ""));
                }
            }

            if (searchTerm.Contains(RightSingleQuotationMark))
            {
                additionalTerms.AppendFormat(" or ( {0} )", searchTerm.Replace(RightSingleQuotationMark, "'"));
                additionalTerms.AppendFormat(" or ( {0} )", searchTerm.Replace(RightSingleQuotationMark, ""));
                if (searchTerm.Contains(RightSingleQuotationMarkWithS))
                {
                    additionalTerms.AppendFormat(" or ( {0} )", searchTerm.Replace(RightSingleQuotationMarkWithS, ""));
                }
            }

            return additionalTerms.ToString();
        }
    }
}