using System;
using System.Text;

namespace R2V2.TextMLExport.Tasks
{
	public class TaskResultStep
	{
		public int Id { get; set; }
		public int TaskResultId { get; set; }
		public string Name { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public bool CompletedSuccessfully { get; set; }
		public StringBuilder Results { get; set; }
		public string FilePath { get; set; }
		public long FileSize { get; set; }
		public DateTime FileTimestamp { get; set; }

		public bool FileWasPreviouslyProcessed { get; set; }

		public TaskResultStep()
		{
			Results = new StringBuilder();
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
			var sb = new StringBuilder("TaskResultStep = [")
				.AppendFormat("Id = {0}", Id)
				.AppendFormat(", TaskResultId = {0}", TaskResultId)
				.AppendFormat(", Name = {0}", Name)
				.AppendFormat(", StartTime = {0:u}", StartTime)
				.AppendFormat(", EndTime = {0:u}", EndTime)
				.AppendFormat(", CompletedSuccessfully = {0}", CompletedSuccessfully)
				.AppendFormat(", FileSize = {0}", FileSize)
				.AppendFormat(", FileTimestamp = {0:u}", FileTimestamp)
				.AppendFormat(", FilePath = {0}", FilePath)
				.AppendFormat(", FileWasPreviouslyProcessed = {0}", FileWasPreviouslyProcessed)
				.AppendFormat(", Results = {0}", Results)
				.Append("]");

			return sb.ToString();
		}
	}
}
