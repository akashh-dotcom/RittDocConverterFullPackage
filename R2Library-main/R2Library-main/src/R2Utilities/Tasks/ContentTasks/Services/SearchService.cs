#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using dtSearch.Engine;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.ContentTasks.Services
{
    public class SearchService : R2UtilitiesBase
    {
        private readonly IContentSettings _contentSettings;

        public SearchService(IContentSettings contentSettings)
        {
            _contentSettings = contentSettings;
        }

        public IList<ISearchResultItem> PerformSearchByIsbn(string isbn)
        {
            var results = new List<ISearchResultItem>();

            var logInfo = new StringBuilder();

            // Set the HomeDir in Options, or stemming won't work
            // (stemming needs to find the stemming.dat file)
            var opts = new Options { HomeDir = @"C:\Program Files\dtSearch Developer\bin" };
            opts.Save();

            using (var searchJob = new SearchJob())
            {
                var searchTimer = new Stopwatch();
                searchTimer.Start();

                searchJob.Request = new StringBuilder()
                    .AppendFormat("(r2isbn contains ( {0} )) or (Filename contains ( {0} ))", isbn)
                    .ToString();

                logInfo.AppendFormat("Request: '{0}'", searchJob.Request);

                // Limit server resources consumed by search
                searchJob.AutoStopLimit = 25000;
                searchJob.TimeoutSeconds = 30;

                // dtsSearchAutoTermWeight and dtsSearchPositionalScoring
                // improve relevancy ranking.  The dtsSearchDelayDocInfo
                // flag makes searching faster.
                searchJob.SearchFlags =
                    SearchFlags.dtsSearchAutoTermWeight |
                    SearchFlags.dtsSearchPositionalScoring |
                    SearchFlags.dtsSearchDelayDocInfo;

                // Specify the path to the index to search here
                var indexPath = _contentSettings.DtSearchIndexLocation;
                searchJob.IndexesToSearch.Add(indexPath);
                //Log.DebugFormat("Index Path: {0}", indexPath);
                //sj.IndexesToSearch.Add("c:\\Program Files\\dtSearch Developer\\UserData\\test2");
                searchJob.Execute();
                searchTimer.Stop();

                // Store the error message in the status
                if (searchJob.Errors.Count > 0)
                {
                    var fullError = searchJob.Errors.Message(0);
                    //fullError = fullError.Substring(fullError.IndexOf(" ") + 1);
                    fullError = fullError.Substring(fullError.IndexOf(" ", StringComparison.Ordinal) + 1);
                    Log.InfoFormat("The search returned an error: {0}", fullError);
                }

                //Log.DebugFormat("searchJob.TaskResults.Count: {0} - {1:0.000 ms}", searchJob.Results.Count, searchTimer.ElapsedMilliseconds);
                logInfo.AppendFormat(", searchJob.TaskResults.Count: {0} - {1:0.000 ms}", searchJob.Results.Count,
                    searchTimer.ElapsedMilliseconds);

                var resultsTimer = new Stopwatch();
                resultsTimer.Start();
                for (var i = 0; i < searchJob.Results.Count; i++)
                {
                    searchJob.Results.GetNthDoc(i);
                    var item = searchJob.Results.CurrentItem;

                    var r2Item = new R2SearchResultItem(item);
                    results.Add(r2Item);
                }

                resultsTimer.Stop();
                //Log.DebugFormat("results.Count: {0} - {1:0.000 ms}", results.Count, resultsTimer.ElapsedMilliseconds);
                logInfo.AppendFormat(", results.Count: {0} - {1:0.000 ms}", results.Count,
                    resultsTimer.ElapsedMilliseconds);
                Log.Debug(logInfo.ToString());
            }

            return results;
        }
    }
}