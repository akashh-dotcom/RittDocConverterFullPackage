#region

using System;
using System.Diagnostics;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.MaintenanceTasks
{
    public class AggregateDailyInstitutionResourceStatisticsTask : TaskBase
    {
        private readonly EmailTaskService _emailTaskService;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly UtilitiesStatisticsService _utilitiesStatisticsService;

        public AggregateDailyInstitutionResourceStatisticsTask(
            UtilitiesStatisticsService utilitiesStatisticsService
            , EmailTaskService emailTaskService
            , IR2UtilitiesSettings r2UtilitiesSettings
        )
            : base("AggregateDailyInstitutionResourceStatisticsTask",
                "-AggregateDailyInstitutionResourceStatisticsTask", "15", TaskGroup.ContentLoading,
                "Task to aggregate daily institution resource data", true)
        {
            _utilitiesStatisticsService = utilitiesStatisticsService;
            _emailTaskService = emailTaskService;
            _r2UtilitiesSettings = r2UtilitiesSettings;
        }

        public DateTime AggregateStartDate { get; set; }
        public int MaxCount { get; set; }

        public override void Run()
        {
            AggregateStartDate = new DateTime(2009, 1, 13);
            MaxCount = 1000;

            TaskResult.Information = "This task will aggregate Institution Resource Statistics.";
            var step = new TaskResultStep { Name = "DailyInstitutionResourceStatisticsTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            var totalAggregateCount = 0;
            var timer = new Stopwatch();
            timer.Start();
            try
            {
                var startDate = _emailTaskService.GetAggregateInstitutionResourceStatisticsStartDate();
                if (startDate != null)
                {
                    AggregateStartDate = startDate.Value;
                }

                var hardEndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                var zeroCounter = 0;
                while (AggregateStartDate < hardEndDate)
                {
                    var subtimer = new Stopwatch();
                    subtimer.Start();
                    var endDate =
                        new DateTime(AggregateStartDate.Year, AggregateStartDate.Month, AggregateStartDate.Day)
                            .AddDays(1);

                    var count =
                        _utilitiesStatisticsService.AggregateInstitutionResourceStatisticsCount(AggregateStartDate,
                            endDate);
                    Log.InfoFormat("Records Aggregated: {0} || Total time: {1} seconds", count,
                        TimeSpan.FromMilliseconds(subtimer.ElapsedMilliseconds).TotalSeconds);

                    totalAggregateCount += count;
                    AggregateStartDate = AggregateStartDate.AddDays(1);

                    if (count == 0)
                    {
                        zeroCounter++;
                    }

                    if (zeroCounter > 7)
                    {
                        break;
                    }
                }

                var rowsUpdated = 0;
                if (_r2UtilitiesSettings.UpdateInstitutionStatisticsPreviousDays > 0)
                {
                    var updateStartDate =
                        new DateTime(hardEndDate.Year, hardEndDate.Month, hardEndDate.Day).AddDays(-_r2UtilitiesSettings
                            .UpdateInstitutionStatisticsPreviousDays);
                    rowsUpdated =
                        _utilitiesStatisticsService.UpdateInstitutionResourceStatisticsCount(updateStartDate,
                            hardEndDate);
                }

                _utilitiesStatisticsService.RebuildAndReorgIndexes();

                var stepResults = new StringBuilder();
                stepResults.AppendFormat("{0} Total items aggregated (INSERT).", totalAggregateCount).AppendLine();
                stepResults.AppendFormat("{0} Total items aggregated (UPDATED).", rowsUpdated).AppendLine();

                stepResults.AppendFormat("Total time: {0} seconds, or {1} min",
                    TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds).TotalSeconds,
                    TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds).TotalMinutes).AppendLine();

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
                Log.InfoFormat("Total time: {0} ms, or {1} min", totalAggregateCount,
                    TimeSpan.FromMilliseconds(totalAggregateCount).TotalMinutes);
                step.EndTime = DateTime.Now;
                UpdateTaskResult();
            }
        }
    }
}