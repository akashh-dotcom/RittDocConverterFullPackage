#region

using System;
using System.Collections.Generic;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;

#endregion

namespace R2Library.Data.ADO.R2Utility.DataServices
{
    public class TaskResultDataService : DataServiceBase
    {
        public int InsertTaskResult(TaskResult taskResult)
        {
            var sql = new StringBuilder()
                .Append(
                    "insert into TaskResult (taskName, taskStartTime, taskEndTime, taskCompletedSuccessfully, taskResults) ")
                .Append("values (@TaskName, @TaskStartTime, @TaskEndTime, @TaskCompletedSuccessfully, @TaskResults)");

            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("TaskName", taskResult.Name),
                new DateTimeParameter("TaskStartTime", taskResult.StartTime),
                new DateTimeNullParameter("TaskEndTime", taskResult.EndTime),
                new BooleanParameter("TaskCompletedSuccessfully", taskResult.CompletedSuccessfully),
                new StringParameter("TaskResults", taskResult.Results)
            };

            var id = ExecuteInsertStatementReturnIdentity(sql.ToString(), parameters.ToArray(), true);
            taskResult.Id = id;
            return id;
        }


        public int UpdateTaskResult(TaskResult taskResult)
        {
            //SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(Properties.Settings.Default.RittenhouseWebDb);
            var sql = new StringBuilder()
                .Append("update TaskResult set ")
                .Append("       taskEndTime = @TaskEndTime ")
                .Append("     , taskCompletedSuccessfully = @TaskCompletedSuccessfully ")
                .Append("     , taskResults = @TaskResults ")
                .Append("where  taskResultId = @TaskResultId");


            var parameters = new List<ISqlCommandParameter>
            {
                new DateTimeNullParameter("TaskEndTime", taskResult.EndTime),
                new BooleanParameter("TaskCompletedSuccessfully", taskResult.CompletedSuccessfully),
                new StringParameter("TaskResults", taskResult.Results),
                new Int32Parameter("TaskResultId", taskResult.Id)
            };

            var rowCount = ExecuteUpdateStatement(sql.ToString(), parameters.ToArray(), true);
            return rowCount;
        }


        public int SaveTaskResultStep(TaskResultStep taskResultStep)
        {
            if (taskResultStep.IsDirty())
            {
                if (taskResultStep.Id > 0)
                {
                    return UpdateTaskResultStep(taskResultStep);
                }

                return InsertTaskResultStep(taskResultStep);
            }

            return 0;
        }

        public int InsertTaskResultStep(TaskResultStep taskResultStep)
        {
            var sql = new StringBuilder()
                .Append(
                    "insert into TaskResultStep (taskResultId, stepName, stepStartTime, stepEndTime, stepCompletedSuccessfully, stepResults) ")
                .Append(
                    "values (@TaskResultId, @StepName, @StepStartTime, @StepEndTime, @StepCompletedSuccessfully, @StepResults)");

            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("TaskResultId", taskResultStep.TaskResult.Id),
                new StringParameter("StepName", taskResultStep.Name),
                new DateTimeParameter("StepStartTime", taskResultStep.StartTime),
                new DateTimeNullParameter("StepEndTime", taskResultStep.EndTime),
                new BooleanParameter("StepCompletedSuccessfully", taskResultStep.CompletedSuccessfully),
                new StringParameter("StepResults", taskResultStep.Results)
            };

            var id = ExecuteInsertStatementReturnIdentity(sql.ToString(), parameters.ToArray(), true);
            taskResultStep.Id = id;

            var info = new StringBuilder()
                .AppendFormat("Id: {0}, CompletedSuccessfully: {1}, Name: {2}", taskResultStep.Id,
                    taskResultStep.CompletedSuccessfully, taskResultStep.Name).AppendLine()
                .AppendFormat("\tStartTime: {0}, EndTime: {1}, Results: {2}", taskResultStep.StartTime,
                    taskResultStep.EndTime, taskResultStep.Results);
            Log.Info(info.ToString());

            taskResultStep.SetTrackChangesHash();

            return id;
        }

        public int UpdateTaskResultStep(TaskResultStep taskResultStep)
        {
            var sql = new StringBuilder()
                .Append(
                    "update TaskResultStep set stepEndTime = @StepEndTime, stepCompletedSuccessfully = @StepCompletedSuccessfully, stepResults = @StepResults ")
                .Append("where  taskResultStepId = @TaskResultStepId;");

            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("TaskResultStepId", taskResultStep.Id),
                new DateTimeNullParameter("StepEndTime", taskResultStep.EndTime),
                new BooleanParameter("StepCompletedSuccessfully", taskResultStep.CompletedSuccessfully),
                new StringParameter("StepResults", taskResultStep.Results)
            };

            var rowCount = ExecuteUpdateStatement(sql.ToString(), parameters.ToArray(), true);

            var info = new StringBuilder()
                .AppendLine(">>>> Step Updated");
            if (taskResultStep.EndTime == null)
            {
                info.AppendFormat("{0} - {1} - {2}", taskResultStep.Id,
                    taskResultStep.CompletedSuccessfully ? "ok" : "processing", taskResultStep.Name);
            }
            else
            {
                info.AppendFormat("{0} - {1} - {2}", taskResultStep.Id,
                    taskResultStep.CompletedSuccessfully ? "ok" : "ERROR", taskResultStep.Name);
            }

            info.AppendLine().AppendFormat("{0}", taskResultStep.Results).AppendLine("<<<<");
            Log.Debug(info.ToString());

            taskResultStep.SetTrackChangesHash();

            return rowCount;
        }

        /// <summary>
        /// </summary>
        public List<TaskResult> GetTaskResultsFromDate(DateTime minDate, DateTime maxDate, int currentTaskResultId)
        {
            var sql = new StringBuilder()
                .Append(
                    "select taskResultId, taskName, taskStartTime, taskEndTime, taskCompletedSuccessfully, taskResults ")
                .Append("from   TaskResult ")
                .Append("where  taskStartTime >= @MinDate ")
                .Append("  and  taskStartTime < @MaxDate ")
                .Append("   and taskResultId <> @TaskResultId ")
                //.Append(" and taskResults != 'Init Complete' ") // The Init Complete Task NEVER completes successfully so I filter it out.
                .Append("order by taskResultId desc ");

            var parameters = new List<ISqlCommandParameter>
            {
                new DateTimeParameter("MinDate", minDate),
                new DateTimeParameter("MaxDate", maxDate),
                new Int32Parameter("TaskResultId", currentTaskResultId)
            };

            var taskResultEntities = GetEntityList<TaskResult>(sql.ToString(), parameters, true);

            sql = new StringBuilder()
                .Append(
                    "select trs.taskResultStepId, trs.taskResultId, trs.stepName, trs.stepStartTime, trs.stepEndTime, trs.stepCompletedSuccessfully, trs.stepResults ")
                .Append("from   TaskResult tr join  dbo.TaskResultStep trs on trs.taskResultId = tr.taskResultId ")
                .Append("where  tr.taskStartTime >= @MinDate ")
                .Append("  and  tr.taskStartTime < @MaxDate ")
                .Append("   and tr.taskResultId <> @TaskResultId ")
                .Append("order by trs.taskResultId, trs.taskResultStepId ");

            var taskResultStepEntities = GetEntityList<TaskResultStep>(sql.ToString(), parameters, true);

            foreach (var taskResult in taskResultEntities)
            {
                foreach (var step in taskResultStepEntities)
                {
                    if (step.TaskResult.Id == taskResult.Id)
                    {
                        if (taskResult.Steps == null)
                        {
                            taskResult.Steps = new List<TaskResultStep>();
                        }

                        taskResult.Steps.Add(step);
                    }
                }
            }

            return taskResultEntities;
        }

        public TaskResult GetPreviousTaskResult(string taskName)
        {
            var sql = new StringBuilder()
                .Append(
                    "select top 1 taskResultId, taskName, taskStartTime, taskEndTime, taskCompletedSuccessfully, taskResults ")
                .Append("from   dbo.TaskResult ")
                .Append("where  taskName = @TaskName ")
                .Append("order by taskResultId desc ");

            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("TaskName", taskName)
            };

            var taskResult = GetFirstEntity<TaskResult>(sql.ToString(), parameters, true);

            sql = new StringBuilder()
//                .Append("select trs.taskResultStepId, trs.taskResultId, trs.stepName, trs.stepStartTime, trs.stepEndTime, trs.stepCompletedSuccessfully, trs.filePath, trs.fileTimestamp, trs.fileSizeInBytes, trs.stepResults ")
                .Append(
                    "select trs.taskResultStepId, trs.taskResultId, trs.stepName, trs.stepStartTime, trs.stepEndTime, trs.stepCompletedSuccessfully, trs.stepResults ")
                .Append("from   dbo.TaskResultStep trs  ")
                .Append("where  trs.taskResultId = @TaskResultId ")
                .Append("order by trs.taskResultStepId ");

            parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("TaskResultId", taskResult.Id)
            };

            var taskResultStepEntities = GetEntityList<TaskResultStep>(sql.ToString(), parameters, true);

            foreach (var step in taskResultStepEntities)
            {
                taskResult.AddStep(step);
            }

            return taskResult;
        }


        public TaskResultStep GetLatestTaskResultStepByName(string stepName)
        {
            var sql = new StringBuilder()
                .Append(
                    "select top 1 taskResultStepId, taskResultId, stepName, stepStartTime, stepEndTime, stepCompletedSuccessfully, filePath, fileTimestamp, fileSizeInBytes, stepResults ")
                .Append("from   TaskResultStep ")
                .Append("where  stepName = @StepName ")
                .Append("order by stepStartTime desc, taskResultStepId desc; ")
                .ToString();


            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("StepName", stepName)
            };

            var step = GetFirstEntity<TaskResultStep>(sql, parameters, true);

            return step;
        }
    }
}