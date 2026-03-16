#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;

#endregion

namespace R2Utilities.Tasks.MaintenanceTasks
{
    public class UpdateContentViewWithPageView : TaskBase
    {
        private readonly ReportDataService _reportDataService;

        public UpdateContentViewWithPageView(ReportDataService reportDataService)
            : base("UpdateContentViewWithPageView", "-UpdateContentViewWithPageView", "10", TaskGroup.ContentLoading,
                "Updates the Content View Table in the R2Reports Database with Search Details", true)
        {
            _reportDataService = reportDataService;
        }

        public override void Run()
        {
            TaskResult.Information =
                "This task will update the Content View Table in the R2Reports Database with Search Details.";
            var step = new TaskResultStep { Name = "UpdateContentViewWithPageView", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                var startDate = DateTime.Parse("9/01/2012 12:00:01 AM");
                var endDate = DateTime.Parse("7/10/2013 23:59:59 PM");

                var pageViews = _reportDataService.GetPageViews(startDate, endDate);
                var contentViews = _reportDataService.GetContentViews(startDate, endDate);

                var foundSearchResourceHits = new Dictionary<ReportContentView, string>();

                ReportPageView lastPageViewWithSearch = null;
                var hitCount = 0;
                var counter = 0;

                if (contentViews.Any() && pageViews.Any())
                {
                    foreach (var reportPageView in pageViews)
                    {
                        counter++;

                        Log.DebugFormat("Processing {0} of {1}   ||    Total Found: {2}", counter, pageViews.Count,
                            hitCount);
                        Console.WriteLine("Processing {0} of {1}   ||    Total Found: {2}", counter, pageViews.Count,
                            hitCount);

                        if (reportPageView.Url.Contains("/Search?q="))
                        {
                            lastPageViewWithSearch = reportPageView;
                            continue;
                        }

                        if (reportPageView.Url.Contains("/Resource/") && lastPageViewWithSearch != null &&
                            lastPageViewWithSearch.SessionId == reportPageView.SessionId)
                        {
                            var view = reportPageView;
                            var contentViewMatch = contentViews.FirstOrDefault(x =>
                                x.InstitutionId == view.InstitutionId &&
                                x.IpAddressInteger == view.IpAddressInteger &&
                                x.ContentViewTimestamp == view.PageViewTimeStamp);

                            if (contentViewMatch != null)
                            {
                                hitCount++;
                                var searchTerm = Regex.Split(lastPageViewWithSearch.Url, "q=").Skip(1).FirstOrDefault();
                                foundSearchResourceHits.Add(contentViewMatch, HttpUtility.UrlDecode(searchTerm));

                                Log.DebugFormat("Found search hit. ContentId : {0} || Search Term: {1}",
                                    contentViewMatch.ContentId, searchTerm);
                                Console.WriteLine("Found search hit. ContentId : {0} || Search Term: {1}",
                                    contentViewMatch.ContentId, searchTerm);
                            }

                            lastPageViewWithSearch = null;
                        }
                    }

                    if (foundSearchResourceHits.Any())
                    {
                        _reportDataService.SaveContentViews(foundSearchResourceHits);
                    }
                }


                var sb = new StringBuilder();


                sb.AppendLine($"{foundSearchResourceHits.Count} searches have been matched with content Retrievals.");

                step.Results = sb.ToString();
                step.CompletedSuccessfully = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                step.CompletedSuccessfully = false;
                step.Results = ex.Message;
                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                UpdateTaskResult();
            }
        }
    }
}