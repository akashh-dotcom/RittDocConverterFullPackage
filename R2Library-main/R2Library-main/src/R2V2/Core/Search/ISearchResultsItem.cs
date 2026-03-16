#region

using System;
using System.Collections.Specialized;

#endregion

namespace R2V2.Core.Search
{
    public interface ISearchResultsItem
    {
        SearchResource SearchResource { get; set; }
        string Isbn { get; set; }
        bool BookHit { get; set; }
        string Chapter { get; set; }
        string BookTitle { get; set; }
        string ChapterTitle { get; set; }
        string ChapterNumber { get; set; }
        string ChapterId { get; set; }
        string SectionTitle { get; set; }
        string SectionId { get; set; }
        string Publisher { get; set; }
        string PrimaryAuthor { get; set; }

        int ItemNumber { get; set; }
        //string Gist { get; set; }

        /// <summary>
        ///     An integer that uniquely identifies each document in an index. (SearchResultsItem.DocId)
        /// </summary>
        int DocumnetId { get; set; }

        /// <summary>
        ///     Creation date of the document (UTC) (SearchResultsItem.CreatedDate)
        /// </summary>
        DateTime CreatedDate { get; set; }

        /// <summary>
        ///     If non-blank, a user-friendly name to display for the document (for example, the title of an HTML document).
        ///     (SearchResultsItem.DisplayName)
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        ///     Name of the document as it is stored in the index. (SearchResultsItem.Filename)
        /// </summary>
        string Filename { get; set; }

        /// <summary>
        ///     Number of hits found in this document. (SearchResultsItem.HitCount)
        /// </summary>
        int HitCount { get; set; }

        /// <summary>
        ///     Details on words matched in this document. (SearchResultsItem.HitDetails)
        /// </summary>
        string[] HitDetails { get; set; }

        /// <summary>
        ///     Word offsets of hits found in this document. (SearchResultsItem.Hits)
        /// </summary>
        int[] Hits { get; set; }

        /// <summary>
        ///     By-word summary of hits found in this document. (SearchResultsItem.HitsByWord)
        /// </summary>
        string[] HitsByWord { get; set; }

        /// <summary>
        ///     dtSearch Engine build number used to index this document (SearchResultsItem.IndexedBy)
        /// </summary>
        int IndexedBy { get; set; }

        /// <summary>
        ///     Full path to the index this document was found in. (SearchResultsItem.IndexRetrievedFrom)
        /// </summary>
        string IndexRetrievedFrom { get; set; }

        /// <summary>
        ///     The folder where the document is located. (SearchResultsItem.Location)
        /// </summary>
        string Location { get; set; }

        /// <summary>
        ///     Modification date of the document (UTC) (SearchResultsItem.ModifiedDate)
        /// </summary>
        DateTime ModifiedDate { get; set; }

        /// <summary>
        ///     Number of hits in this document, counting each phrase as one hit. This is only available if the
        ///     dtsSearchWantHitsByWord flag was set in SearchJob.SearchFlags. (SearchResultsItem.PhraseCount)
        /// </summary>
        int PhraseCount { get; set; }

        /// <summary>
        ///     Relevance score for this document. (SearchResultsItem.Score)
        /// </summary>
        int Score { get; set; }

        /// <summary>
        ///     Relevance score for this document, expressed as a percentage (0-100) of the highest-scoring document in the search.
        ///     (SearchResultsItem.ScorePercent)
        /// </summary>
        int ScorePercent { get; set; }

        /// <summary>
        ///     The name of the document, without the path. (SearchResultsItem.ShortName)
        /// </summary>
        string ShortName { get; set; }

        /// <summary>
        ///     Size of this document when it was indexed. (SearchResultsItem.Size)
        /// </summary>
        int Size { get; set; }

        /// <summary>
        ///     Hits-in-context string from SearchReportJob. (SearchResultsItem.Synopsis)
        /// </summary>
        string Synopsis { get; set; }

        /// <summary>
        ///     The first 80 text characters of the document. (SearchResultsItem.Title)
        /// </summary>
        string Title { get; set; }

        /// <summary>
        ///     Field-value pairs found in this document when the document was indexed. (SearchResultsItem.UserFields)
        /// </summary>
        StringDictionary UserFields { get; set; }

        /// <summary>
        ///     Use to prevent an item from being added to SearchResults in the OnFound callback. (SearchResultsItem.VetoThisItem)
        /// </summary>
        bool VetoThisItem { get; set; }

        /// <summary>
        ///     Integer identifying the index that the document was retrieved from, in the IndexesToSearch array
        ///     (SearchResultsItem.WhichIndex)
        /// </summary>
        int WhichIndex { get; set; }

        /// <summary>
        ///     Number of words in this document when it was indexed. (SearchResultsItem.WordCount)
        /// </summary>
        int WordCount { get; set; }

        string ToString();
    }
}