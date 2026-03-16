#region

using System;
using System.Data.SqlClient;
using System.Text;
using R2Library.Data.ADO.Core;

#endregion

namespace R2Library.Data.ADO.R2Utility
{
    public class TaskResultStep : TrackChangesFactoryBase, IDataEntity
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime StartTime { get; set; }
        public virtual DateTime? EndTime { get; set; }
        public virtual bool CompletedSuccessfully { get; set; }
        public virtual string Results { get; set; }
        public virtual bool HasWarnings { get; set; }

        public virtual TaskResult TaskResult { get; set; }

        public virtual string Status => CompletedSuccessfully ? HasWarnings ? "WARNING" : "Ok" : "ERROR";


        public void Populate(SqlDataReader reader)
        {
            try
            {
                Id = GetInt32Value(reader, "taskResultStepId", -1);
                Name = GetStringValue(reader, "stepName");
                StartTime = GetDateValue(reader, "stepStartTime");
                EndTime = GetDateValueOrNull(reader, "stepEndTime");
                CompletedSuccessfully = GetBoolValue(reader, "stepCompletedSuccessfully", false);
                Results = GetStringValue(reader, "stepResults");
                TaskResult = new TaskResult { Id = GetInt32Value(reader, "taskResultId", -1) };
            }
            catch (Exception ex)
            {
                Log.ErrorFormat(ex.Message, ex);
                throw;
            }
        }

        public TimeSpan GetRunTime()
        {
            if (EndTime == null)
            {
                return new TimeSpan(0, 0, 0, 0, 0);
            }

            return (DateTime)EndTime - StartTime;
        }

        protected override string GetStringToHash()
        {
            return ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder("TaskResultStep = [")
                .AppendFormat("Id = {0}", Id)
                .AppendFormat(", TaskResult.Id = {0}", TaskResult.Id)
                .AppendFormat(", Name = {0}", Name)
                .AppendFormat(", StartTime = {0:u}", StartTime)
                .AppendFormat(", EndTime = {0:u}", EndTime)
                .AppendFormat(", CompletedSuccessfully = {0}", CompletedSuccessfully)
                .AppendFormat(", Results = {0}", Results)
                .AppendFormat(", HasWarnings = {0}", HasWarnings)
                .AppendFormat(", Status = {0}", Status)
                .Append("]");

            return sb.ToString();
        }
    }
}