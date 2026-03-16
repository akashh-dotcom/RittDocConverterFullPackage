#region

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;
using R2Library.Data.ADO.Core;

#endregion

namespace R2Library.Data.ADO.R2Utility
{
    public class TaskResult : FactoryBase, IDataEntity
    {
        protected new static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime StartTime { get; set; }
        public virtual DateTime? EndTime { get; set; }
        public virtual bool CompletedSuccessfully { get; set; }
        public virtual string Results { get; set; }
        public virtual string Information { get; set; }

        public virtual IList<TaskResultStep> Steps { get; set; }

        public virtual string EmailAttachmentData { get; set; }

        public virtual bool HasWarnings
        {
            get { return Steps != null && Steps.Any(s => s.HasWarnings); }
        }

        public virtual string Status => CompletedSuccessfully ? HasWarnings ? "WARNING" : "Ok" : "ERROR";

        public void Populate(SqlDataReader reader)
        {
            try
            {
                Id = GetInt32Value(reader, "taskResultId", -1);
                Name = GetStringValue(reader, "taskName");
                StartTime = GetDateValue(reader, "taskStartTime");
                EndTime = GetDateValueOrNull(reader, "taskEndTime");
                CompletedSuccessfully = GetBoolValue(reader, "taskCompletedSuccessfully", false);
                Results = GetStringValue(reader, "taskResults");
            }
            catch (Exception ex)
            {
                Log.ErrorFormat(ex.Message, ex);
                throw;
            }
        }

        public virtual void AddStep(TaskResultStep step)
        {
            if (step.Id <= 0)
            {
                var info = new StringBuilder()
                    .AppendFormat(">>>> Step Started - {0} <<<<", step.Name);

                Log.Info(info.ToString());
            }

            if (Steps == null)
            {
                Steps = new List<TaskResultStep>();
            }

            step.TaskResult = this;
            Steps.Add(step);
        }

        public virtual TimeSpan GetRunTime()
        {
            if (EndTime == null)
            {
                return new TimeSpan(0, 0, 0, 0, 0);
            }

            return (DateTime)EndTime - StartTime;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("TaskResult = [")
                .AppendFormat("Id = {0}", Id)
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