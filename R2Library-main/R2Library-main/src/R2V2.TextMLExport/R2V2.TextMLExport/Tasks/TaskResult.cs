using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using log4net;

namespace R2V2.TextMLExport.Tasks
{
	public class TaskResult
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

		public int Id { get; set; }
		public string Name { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public bool CompletedSuccessfully { get; set; }
		public string RunComments { get; set; }

		public List<TaskResultStep> Steps { get; set; }

		public TaskResult()
		{
			Steps = new List<TaskResultStep>();
		}

		public void AddStep(TaskResultStep step)
		{
			if (step.Id <= 0)
			{
				var info = new StringBuilder()
					.AppendFormat(">>>> Step Started - {0} <<<<", step.Name);

				Log.Info(info.ToString());
			}
			Steps.Add(step);
		}

		public TimeSpan GetRunTime()
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
				.AppendFormat(", RunComments = {0}", RunComments)
				.Append("]");

			return sb.ToString();
		}


	}
}
