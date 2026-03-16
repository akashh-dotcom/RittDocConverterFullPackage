#region

using System;
using System.Collections.Specialized;
using System.Text;

#endregion

namespace R2V2.Core.Search
{
    public class SearchResultsItem : ISearchResultsItem
    {
        public SearchResource SearchResource { get; set; }

        public string Isbn { get; set; }
        public bool BookHit { get; set; }
        public string Chapter { get; set; }
        public string BookTitle { get; set; }
        public string ChapterTitle { get; set; }
        public string ChapterNumber { get; set; }
        public string ChapterId { get; set; }
        public string SectionTitle { get; set; }
        public string SectionId { get; set; }
        public string Publisher { get; set; }
        public string PrimaryAuthor { get; set; }

        public int ItemNumber { get; set; }
        //public string Gist { get; set; }

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
        ///     Name of the document as it is stored in the index. (SearchResultsItem.Filename)
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

        ///// <summary>
        ///// Type id of the document (SearchResultsItem.TypeId)
        ///// </summary>
        //public TypeId TypeId { get; set; }

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

        public override string ToString()
        {
            var sb = new StringBuilder("SearchResultsItem = [");
            sb.AppendFormat("Isbn: {0}", Isbn);
            sb.AppendFormat(", BookHit: {0}", BookHit);
            sb.AppendFormat(", Chapter: {0}", Chapter);
            sb.AppendFormat(", SearchResource: {0}", SearchResource != null ? SearchResource.Resource.Id : -1);
            sb.AppendFormat(", DocumnetId: {0}", DocumnetId).AppendLine();

            sb.AppendFormat("\t, DisplayName: {0}", DisplayName);
            sb.AppendFormat(", ShortName: {0}", ShortName);
            sb.AppendFormat(", Filename: {0}", Filename);
            sb.AppendFormat(", Title: {0}", Title).AppendLine();

            sb.AppendFormat("\t, HitCount: {0}", HitCount);
            sb.AppendFormat(", HitsByWord: {0}", HitsByWord != null ? string.Join("; ", HitsByWord) : string.Empty);
            sb.AppendFormat(", PhraseCount: {0}", PhraseCount).AppendLine();

            //sb.AppendFormat(", HitDetails: {0}", (HitDetails != null) ? string.Join("; ", HitDetails) : string.Empty).AppendLine();
            sb.AppendFormat("\t, HitDetails:\r\n\t\t{0}",
                HitDetails != null ? string.Join(";\r\n\t\t", HitDetails) : string.Empty).AppendLine();


            sb.AppendFormat("\t, Score: {0}", Score);
            sb.AppendFormat(", ScorePercent: {0}", ScorePercent);
            sb.AppendFormat(", WordCount: {0}", WordCount);
            sb.AppendFormat(", Size: {0}", Size).AppendLine();

            if (UserFields != null)
            {
                sb.Append("\t, UserFields:");
                foreach (string key in UserFields.Keys)
                {
                    sb.AppendFormat(" {0} = {1};", key, UserFields[key]);
                }

                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("\t, UserFields: ");
            }

            sb.AppendFormat("\t, VetoThisItem: {0}", VetoThisItem);
            sb.AppendFormat(", IndexedBy: {0}", IndexedBy);
            sb.AppendFormat(", IndexRetrievedFrom: {0}", IndexRetrievedFrom);
            sb.AppendFormat(", Location: {0}", Location);
            sb.AppendFormat(", WhichIndex: {0}", WhichIndex);
            sb.AppendFormat(", CreatedDate: {0}", CreatedDate);
            sb.AppendFormat(", ModifiedDate: {0}", ModifiedDate).AppendLine();

            sb.AppendFormat("\t, Synopsis: {0}", Synopsis).AppendLine();
            sb.Append("]");
            return sb.ToString();
        }

        public string GetGist(int maxLength)
        {
            if (BookHit || string.IsNullOrEmpty(Synopsis))
            {
                return SearchResource.Resource.GetGist(maxLength);
            }

            if (Synopsis.Length <= maxLength)
            {
                return Synopsis;
            }

            var gist = Synopsis.Substring(0, maxLength);
            for (var i = maxLength - 1 - 1; i >= 0; i--)
            {
                if (gist[i] == ' ')
                {
                    return $"{gist.Substring(0, i)}...";
                }
            }

            return gist;
        }
    }
}