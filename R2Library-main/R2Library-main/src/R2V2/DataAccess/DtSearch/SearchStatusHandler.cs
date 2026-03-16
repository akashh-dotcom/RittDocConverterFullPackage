#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using dtSearch.Engine;
using R2V2.Core.Search;
using R2V2.Core.Search.FacetData;
using SearchResultsItem = dtSearch.Engine.SearchResultsItem;

#endregion

namespace R2V2.DataAccess.DtSearch
{
    public class SearchStatusHandler : ISearchStatusHandler
    {
        private readonly bool _fullTextOnlySearch;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public SearchStatusHandler(bool fullTextOnlySearch)
        {
            _fullTextOnlySearch = fullTextOnlySearch;
        }

        public int FullTextHits { get; private set; }
        public int BookTitleHits { get; private set; }
        public int ChapterTitleHits { get; private set; }
        public int SectionTitleHits { get; private set; }
        public int AuthorHits { get; private set; }
        public int ImageHits { get; private set; }
        public int VideoHits { get; private set; }
        public int IndexedTermHits { get; private set; }
        public int TotalHits { get; private set; }

        void ISearchStatusHandler.OnFound(SearchResultsItem item)
        {
            _stopwatch.Start();
            var hitDetails = item.HitDetails;

            var fullTextHit = false;
            var indexedTermHit = false;
            var bookTitleHit = false;
            var chapterTitleHit = false;
            var sectionTitleHit = false;
            var authorHit = false;
            var imageHit = false;
            var videoHit = false;

            if (hitDetails != null)
            {
                for (var i = 0; i < hitDetails.Length; i++)
                {
                    var field = GetFieldValue(hitDetails[i]);

                    if (string.IsNullOrEmpty(field))
                    {
                        fullTextHit = true;
                        continue;
                    }

                    if (field == "r2IndexTerms")
                    {
                        indexedTermHit = true;
                    }
                    else if (field.Contains("r2BookTitle") || field.Contains("r2BookSubTitle"))
                    {
                        bookTitleHit = true;
                    }
                    else if (field == "r2ChapterTitle")
                    {
                        chapterTitleHit = true;
                    }
                    else if (field == "r2SectionTitle")
                    {
                        sectionTitleHit = true;
                    }
                    else if (field.Contains("r2Author") || field.Contains("r2PrimaryAuthor"))
                    {
                        authorHit = true;
                    }
                    else if (hitDetails[i].Contains("field=\"R2imagetitle\""))
                    {
                        imageHit = true;
                    }
                    else if (hitDetails[i].Contains("field=\"R2videosection\""))
                    {
                        videoHit = true;
                    }
                    else
                    {
                        fullTextHit = true;
                    }
                }
            }

            if (!_fullTextOnlySearch)
            {
                if (indexedTermHit)
                {
                    IndexedTermHits++;
                }

                if (bookTitleHit)
                {
                    BookTitleHits++;
                }

                if (chapterTitleHit)
                {
                    ChapterTitleHits++;
                }

                if (sectionTitleHit)
                {
                    SectionTitleHits++;
                }

                if (authorHit)
                {
                    AuthorHits++;
                }

                if (imageHit)
                {
                    ImageHits++;
                }

                if (videoHit)
                {
                    VideoHits++;
                }

                if (fullTextHit)
                {
                    FullTextHits++;
                }
            }
            else
            {
                if (fullTextHit)
                {
                    FullTextHits++;
                }
                else
                {
                    item.VetoThisItem = true;
                }
            }

            TotalHits++;
            _stopwatch.Stop();
        }

        void ISearchStatusHandler.OnSearchingIndex(string index)
        {
        }

        void ISearchStatusHandler.OnSearchingFile(string filename)
        {
        }

        AbortValue ISearchStatusHandler.CheckForAbort()
        {
            return AbortValue.Continue;
        }

        private string GetFieldValue(string hitDetails)
        {
            var x = hitDetails.IndexOf("field=\"", StringComparison.Ordinal);
            if (x < 0)
            {
                return null;
            }

            x += 7;
            var y = hitDetails.IndexOf("\" ", x, StringComparison.Ordinal);
            var field = y > x ? hitDetails.Substring(x, y - x) : string.Empty;
            return field;
        }

        public IEnumerable<IFacetData> GetSearchResultsCounts()
        {
            var counts = new List<IFacetData>
            {
                //new FieldFacetData {Count = TotalHits, Id = (int)SearchFields.All, Name = "All"},
                new FieldFacetData
                {
                    Count = IndexedTermHits,
                    Id = (int)SearchFields.IndexTerms,
                    Name = "Indexed Terms",
                    Code = $"{SearchFields.IndexTerms}"
                },
                new FieldFacetData
                {
                    Count = FullTextHits,
                    Id = (int)SearchFields.FullText,
                    Name = "Full Text",
                    Code = $"{SearchFields.FullText}"
                },
                new FieldFacetData
                {
                    Count = BookTitleHits,
                    Id = (int)SearchFields.BookTitle,
                    Name = "Book Title",
                    Code = $"{SearchFields.BookTitle}"
                },
                new FieldFacetData
                {
                    Count = ChapterTitleHits,
                    Id = (int)SearchFields.ChapterTitle,
                    Name = "Chapter Title",
                    Code = $"{SearchFields.ChapterTitle}"
                },
                new FieldFacetData
                {
                    Count = SectionTitleHits,
                    Id = (int)SearchFields.SectionTitle,
                    Name = "Section Title",
                    Code = $"{SearchFields.SectionTitle}"
                },
                new FieldFacetData
                {
                    Count = AuthorHits,
                    Id = (int)SearchFields.Author,
                    Name = "Authors",
                    Code = $"{SearchFields.Author}"
                },
                new FieldFacetData
                {
                    Count = ImageHits,
                    Id = (int)SearchFields.ImageTitle,
                    Name = "Images",
                    Code = $"{SearchFields.ImageTitle}"
                },
                new FieldFacetData
                {
                    Count = VideoHits,
                    Id = (int)SearchFields.VideoSection,
                    Name = "Videos",
                    Code = $"{SearchFields.VideoSection}"
                }
            };
            return counts;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("SearchStatusHandler = [");
            sb.AppendFormat("FullTextHits: {0}", FullTextHits);
            sb.AppendFormat(", IndexedTermHits: {0}", IndexedTermHits);
            sb.AppendFormat(", BookTitleHits: {0}", BookTitleHits);
            sb.AppendFormat(", ChapterTitleHits: {0}", ChapterTitleHits);
            sb.AppendFormat(", SectionTitleHits: {0}", SectionTitleHits);
            sb.AppendFormat(", AuthorHits: {0}", AuthorHits);
            sb.AppendFormat(", ImageHits: {0}", ImageHits);
            sb.AppendFormat(", VideoHits: {0}", VideoHits);
            sb.AppendFormat(", TotalHits: {0}", TotalHits);
            sb.AppendFormat(", _stopwatch: {0:c}", _stopwatch.Elapsed);
            sb.Append("]");

            return sb.ToString();
        }
    }
}