#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Search;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.DataAccess.DtSearch
{
    public class SearchResultItemFactory
    {
        private readonly ILog<SearchResultItemFactory> _log;

        public SearchResultItemFactory(ILog<SearchResultItemFactory> log)
        {
            _log = log;
        }

        public SearchResultsItem GetSearchResultsItem(dtSearch.Engine.SearchResultsItem dtSearchItem,
            IEnumerable<SearchResource> searchResources, int itemNumber
            , ISearchRequest searchRequest)
        {
            var item = new SearchResultsItem();
            try
            {
                item.DocumnetId = dtSearchItem.DocId;
                item.CreatedDate = dtSearchItem.CreatedDate;
                item.DisplayName = dtSearchItem.DisplayName;
                item.Filename = dtSearchItem.Filename;
                item.HitCount = dtSearchItem.HitCount;
                item.HitDetails = dtSearchItem.HitDetails;
                item.Hits = dtSearchItem.Hits;
                item.HitsByWord = dtSearchItem.HitsByWord;
                item.IndexedBy = dtSearchItem.IndexedBy;
                item.IndexRetrievedFrom = dtSearchItem.IndexRetrievedFrom;
                item.Location = dtSearchItem.Location;
                item.ModifiedDate = dtSearchItem.ModifiedDate;
                item.PhraseCount = dtSearchItem.PhraseCount;
                item.Score = dtSearchItem.Score;
                item.ScorePercent = dtSearchItem.ScorePercent;
                item.ShortName = dtSearchItem.ShortName;
                item.Size = dtSearchItem.Size;
                item.Synopsis = dtSearchItem.Synopsis;
                item.Title = dtSearchItem.Title;
                item.UserFields = dtSearchItem.UserFields;
                item.VetoThisItem = dtSearchItem.VetoThisItem;
                item.WhichIndex = dtSearchItem.WhichIndex;
                item.WordCount = dtSearchItem.WordCount;

                var parts = item.DisplayName.Split('.');
                item.Isbn = parts[1];
                item.BookHit = parts[0] == "r2BookSearch";
                item.Chapter = parts.Length == 4 ? parts[2] : null;
                item.ItemNumber = itemNumber;

                IList<SearchResource> searchResourceList = searchResources.ToList();
                item.SearchResource = searchResourceList.FirstOrDefault(x => x.Resource.Isbn == item.Isbn);
                var isbns = new StringBuilder();
                foreach (var searchResource in searchResourceList)
                {
                    if (searchResource.Resource.Isbn != item.Isbn)
                    {
                        isbns.AppendFormat("{0},", searchResource.Resource.Isbn);
                    }
                }

                item.BookTitle = dtSearchItem.UserFields["r2BookTitle"];
                item.Publisher = dtSearchItem.UserFields["r2Publisher"];
                item.PrimaryAuthor = dtSearchItem.UserFields["r2AuthorPrimary"];

                item.ChapterId = dtSearchItem.UserFields["r2ChapterId"];
                item.ChapterNumber = dtSearchItem.UserFields["r2ChapterNumber"];
                item.ChapterTitle = dtSearchItem.UserFields["r2ChapterTitle"];
                item.SectionId = dtSearchItem.UserFields["r2SectionId"];
                item.SectionTitle = dtSearchItem.UserFields["r2SectionTitle"];
            }
            catch (Exception ex)
            {
                // SJS - 8/23/2012 - log the exception and ignore this item.
                var msg = new StringBuilder();
                msg.AppendLine(ex.Message);
                msg.AppendFormat("\tDisplayName: {0}", dtSearchItem.DisplayName).AppendLine();
                msg.AppendFormat("\tDicId: {0}", dtSearchItem.DocId).AppendLine();
                msg.AppendFormat("\tDicId: {0}", searchRequest.SearchTerm).AppendLine();
                _log.Error(msg, ex);
                return null;
            }

            return item;
        }
    }
}