#region

using System;
using System.Diagnostics;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2V2.Core.Reports;

#endregion

namespace R2Utilities.Tasks.MaintenanceTasks
{
    public class AggregateInstitutionStatisticsTask : TaskBase
    {
        private readonly DashboardService _dashboardService;
        private readonly UtilitiesStatisticsService _utilitiesStatisticsService;

        public AggregateInstitutionStatisticsTask(
            UtilitiesStatisticsService utilitiesStatisticsService
            , DashboardService dashboardService)
            : base("AggregateInstitutionStatisticsTask", "-AggregateInstitutionStatisticsTask", "14",
                TaskGroup.ContentLoading, "Aggregates institution data for Dashboard Statistics", true)
        {
            _utilitiesStatisticsService = utilitiesStatisticsService;
            _dashboardService = dashboardService;
        }

        public override void Run()
        {
            TaskResult.Information = "This task will aggregate Institution Statistics.";
            var step = new TaskResultStep { Name = "InstitutionStatisticsTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            //Aggregate Data After 2009-01-14
            var totalResourcesInserted = 0;
            var totalInstitutionMonthsAggregated = 0;

            try
            {
                var institutionStatisticsList = _dashboardService.GetInstitutionsForStatistics();
                foreach (var institutionStatistics in institutionStatisticsList)
                {
                    var timer = new Stopwatch();
                    timer.Start();

                    var institutionStatisticsToRun = institutionStatistics;
                    var maxDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    while (institutionStatisticsToRun.StartDate != maxDate)
                    {
                        var subTimer = new Stopwatch();
                        subTimer.Start();
                        var tryCount = 0;
                        var success = false;
                        try
                        {
                            //Will try 3 times to update stats
                            while (!success && tryCount < 3)
                            {
                                institutionStatisticsToRun =
                                    _dashboardService.GetAggregatedInstitutionStatistics(institutionStatisticsToRun);
                                success = _utilitiesStatisticsService.InsertInstitutionStatistics(
                                    institutionStatisticsToRun);
                                if (success)
                                {
                                    totalInstitutionMonthsAggregated++;
                                    Log.InfoFormat("Stats for Institution: {0}. Date Aggregated :{1} it took: {2}ms",
                                        institutionStatisticsToRun.InstitutionId, institutionStatisticsToRun.StartDate,
                                        subTimer.ElapsedMilliseconds);

                                    subTimer.Restart();

                                    totalResourcesInserted +=
                                        _utilitiesStatisticsService.InsertMonthlyResourceStatistics(
                                            institutionStatisticsToRun);

                                    institutionStatisticsToRun = new InstitutionStatistics
                                    {
                                        InstitutionId = institutionStatistics.InstitutionId,
                                        StartDate = institutionStatisticsToRun.StartDate.AddMonths(1)
                                    };
                                }
                                else
                                {
                                    Log.InfoFormat("FAIL!!! -- InstitutionId: {0}, StatisticStartDate: {1}",
                                        institutionStatisticsToRun.InstitutionId, institutionStatisticsToRun.StartDate);
                                }

                                tryCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Info(ex.Message, ex);
                        }

                        subTimer.Restart();
                    }

                    Log.InfoFormat("It took: {1}ms to Aggrate All Stats for Institution {0}",
                        institutionStatisticsToRun.InstitutionId, timer.ElapsedMilliseconds);
                    timer.Restart();
                }

                var stepResults = new StringBuilder();
                stepResults.AppendFormat("{0} Total institutions aggregated.", totalResourcesInserted).AppendLine();
                stepResults.AppendFormat("{0} Total months aggregated for all institutions.",
                    totalInstitutionMonthsAggregated).AppendLine();
                stepResults.AppendFormat("{0} Total resources aggregated for institutions.", totalResourcesInserted)
                    .AppendLine();

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
    }
}