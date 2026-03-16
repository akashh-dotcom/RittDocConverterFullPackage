using System;
using System.Diagnostics;
using System.Text;
using Common.Logging;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;

namespace R2Utilities.Tasks.MaintenanceTasks
{
    public class InstitutionStatisticsTask : TaskBase
    {
        private readonly WebStatisticsService _webStatisticsService;
        private readonly UtilitiesStatisticsService _utilitiesStatisticsService;

        public InstitutionStatisticsTask(WebStatisticsService webStatisticsService, UtilitiesStatisticsService utilitiesStatisticsService)
            : base("InstitutionStatisticsTask")
        {
            _webStatisticsService = webStatisticsService;
            _utilitiesStatisticsService = utilitiesStatisticsService;
        }

        public override void Run()
        {
            TaskResult.Information = "This task will aggregate Institution Statistics.";
            TaskResultStep step = new TaskResultStep { Name = "InstitutionStatisticsTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            //Aggregate Data After 2009-01-14
            int totalResourcesInserted = 0;
            int totalInstitutionMonthsAggregated = 0;

            try
            {
                var institutionStatisticsList = _webStatisticsService.GetInstitutionsForStatistics();
                foreach (var institutionStatistics in institutionStatisticsList)
                {
                    Stopwatch timer = new Stopwatch();
                    timer.Start();

                    var institutionStatisticsToRun = institutionStatistics;
                    var maxDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    while (institutionStatisticsToRun.AggregationDate != maxDate)
                    {
                        Stopwatch subTimer = new Stopwatch();
                        subTimer.Start();

                        _webStatisticsService.SetInstitutionStatistics(institutionStatisticsToRun);
                        var success = _utilitiesStatisticsService.InsertInstitutionStatistics(institutionStatisticsToRun);
                        if (success)
                        {
                            totalInstitutionMonthsAggregated++;
                            Log.InfoFormat("Stats for Institution: {0}. Date Aggregated :{1} it took: {2}ms",
                                institutionStatisticsToRun.InstitutionId, institutionStatisticsToRun.AggregationDate, subTimer.ElapsedMilliseconds);
                            subTimer.Restart();

                            totalResourcesInserted += _utilitiesStatisticsService.InsertMonthlyResourceStatistics(institutionStatisticsToRun);
                            
                            institutionStatisticsToRun = new InstitutionStatistics
                            {
                                InstitutionId = institutionStatistics.InstitutionId,
                                AggregationDate = institutionStatisticsToRun.AggregationDate.AddMonths(1)
                            };
                            
                        }
                        else
                        {
                            Log.InfoFormat("FAIL!!! -- InstitutionId: {0}, StatisticStartDate: {1}",
                                institutionStatisticsToRun.InstitutionId, institutionStatisticsToRun.AggregationDate);
                        }
                        subTimer.Restart();
                    }
                    Log.InfoFormat("It took: {1}ms to Aggrate All Stats for Institution {0}", institutionStatisticsToRun.InstitutionId, timer.ElapsedMilliseconds);
                    timer.Restart();

                }

                var stepResults = new StringBuilder();
                stepResults.AppendFormat("{0} Total institutions aggregated.", totalResourcesInserted).AppendLine();
                stepResults.AppendFormat("{0} Total months aggregated for all institutions.", totalInstitutionMonthsAggregated).AppendLine();
                stepResults.AppendFormat("{0} Total resources aggregated for institutions.", totalResourcesInserted).AppendLine();

                step.Results = stepResults.ToString();
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

        public override void InitTask()
        {
        }
    }
}
