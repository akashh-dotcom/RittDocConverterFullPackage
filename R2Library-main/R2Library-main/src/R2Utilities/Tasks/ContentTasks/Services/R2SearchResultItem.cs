#region

using System;
using System.Collections.Specialized;
using dtSearch.Engine;

#endregion

namespace R2Utilities.Tasks.ContentTasks.Services
{
    public class R2SearchResultItem : ISearchResultItem
    {
        public R2SearchResultItem(SearchResultsItem item)
        {
            DocumnetId = item.DocId;
            CreatedDate = item.CreatedDate;
            DisplayName = item.DisplayName;
            Filename = item.Filename;
            HitCount = item.HitCount;
            HitDetails = item.HitDetails;
            Hits = item.Hits;
            HitsByWord = item.HitsByWord;
            IndexedBy = item.IndexedBy;
            IndexRetrievedFrom = item.IndexRetrievedFrom;
            Location = item.Location;
            ModifiedDate = item.ModifiedDate;
            PhraseCount = item.PhraseCount;
            Score = item.Score;
            ScorePercent = item.ScorePercent;
            ShortName = item.ShortName;
            Size = item.Size;
            Synopsis = item.Synopsis;
            Title = item.Title;
            TypeId = item.TypeId;
            UserFields = item.UserFields;
            VetoThisItem = item.VetoThisItem;
            WhichIndex = item.WhichIndex;
            WordCount = item.WordCount;
        }

        /// <summary>
        ///     An integer that uniquely identifies each document in an index. (SearchResultsItem.DocId)
        /// </summary>
        public int DocumnetId { get; set; }

        /// <summary>
        ///     Creation date of the document (UTC) (SearchResultsItem.CreatedDate)
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        ///     If non-blank, a user-friendly name to display for the document (for example, the title of an HTML document).
        ///     (SearchResultsItem.DisplayName)
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        ///     TaskName of the document as it is stored in the index. (SearchResultsItem.Filename)
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        ///     Number of hits found in this document. (SearchResultsItem.HitCount)
        /// </summary>
        public int HitCount { get; set; }

        /// <summary>
        ///     Details on words matched in this document. (SearchResultsItem.HitDetails)
        /// </summary>
        public string[] HitDetails { get; set; }

        /// <summary>
        ///     Word offsets of hits found in this document. (SearchResultsItem.Hits)
        /// </summary>
        public int[] Hits { get; set; }

        /// <summary>
        ///     By-word summary of hits found in this document. (SearchResultsItem.HitsByWord)
        /// </summary>
        public string[] HitsByWord { get; set; }

        /// <summary>
        ///     dtSearch Engine build number used to index this document (SearchResultsItem.IndexedBy)
        /// </summary>
        public int IndexedBy { get; set; }

        /// <summary>
        ///     Full path to the index this document was found in. (SearchResultsItem.IndexRetrievedFrom)
        /// </summary>
        public string IndexRetrievedFrom { get; set; }

        /// <summary>
        ///     The folder where the document is located. (SearchResultsItem.Location)
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        ///     Modification date of the document (UTC) (SearchResultsItem.ModifiedDate)
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        ///     Number of hits in this document, counting each phrase as one hit. This is only available if the
        ///     dtsSearchWantHitsByWord flag was set in SearchJob.SearchFlags. (SearchResultsItem.PhraseCount)
        /// </summary>
        public int PhraseCount { get; set; }

        /// <summary>
        ///     Relevance score for this document. (SearchResultsItem.Score)
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        ///     Relevance score for this document, expressed as a percentage (0-100) of the highest-scoring document in the search.
        ///     (SearchResultsItem.ScorePercent)
        /// </summary>
        public int ScorePercent { get; set; }

        /// <summary>
        ///     The name of the document, without the path. (SearchResultsItem.ShortName)
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        ///     Size of this document when it was indexed. (SearchResultsItem.Size)
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        ///     Hits-in-context string from SearchReportJob. (SearchResultsItem.Synopsis)
        /// </summary>
        public string Synopsis { get; set; }

        /// <summary>
        ///     The first 80 text characters of the document. (SearchResultsItem.Title)
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     Type id of the document (SearchResultsItem.TypeId)
        /// </summary>
        public TypeId TypeId { get; set; }

        /// <summary>
        ///     Field-value pairs found in this document when the document was indexed. (SearchResultsItem.UserFields)
        /// </summary>
        public StringDictionary UserFields { get; set; }

        /// <summary>
        ///     Use to prevent an item from being added to SearchResults in the OnFound callback. (SearchResultsItem.VetoThisItem)
        /// </summary>
        public bool VetoThisItem { get; set; }

        /// <summary>
        ///     Integer identifying the index that the document was retrieved from, in the IndexesToSearch array
        ///     (SearchResultsItem.WhichIndex)
        /// </summary>
        public int WhichIndex { get; set; }

        /// <summary>
        ///     Number of words in this document when it was indexed. (SearchResultsItem.WordCount)
        /// </summary>
        public int WordCount { get; set; }
    }
}