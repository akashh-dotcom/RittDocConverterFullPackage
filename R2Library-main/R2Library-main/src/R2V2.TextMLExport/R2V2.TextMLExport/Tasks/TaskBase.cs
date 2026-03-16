using System;
using System.Reflection;
using log4net;

//using log4net;

namespace R2V2.TextMLExport.Tasks
{
	public abstract class TaskBase :  ITask
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

		public TaskResult TaskResult { get; private set; }

		protected TaskBase(string taskName)
		{
			try
			{
				TaskResult = new TaskResult { Name = taskName, StartTime = DateTime.Now, RunComments = "Init Complete" };
				Log.Debug(TaskResult);
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message, ex);
				throw;
			}
		}

		public abstract void Run();

		public void Cleanup()
		{
			TaskResult.EndTime = DateTime.Now;
			UpdateTaskResult();
		}

		protected void UpdateTaskResult()
		{
			//try
			//{
			//    using (ISession utilitiesSession = UtilitySessionFactory.OpenSession())
			//    {
			//        using (ITransaction transaction = utilitiesSession.BeginTransaction())
			//        {
			//            utilitiesSession.Save(TaskResult);
			//            transaction.Commit();
			//        }
			//    }
			//}
			//catch (Exception ex)
			//{
			//    Log.Error(ex.Message, ex);
			//    throw;
			//}
		}

		///// <summary>
		/////
		///// </summary>
		///// <param name="step"></param>
		//protected void UpdateTaskResultStep(TaskResultStep step)
		//{
		//    try
		//    {
		//        using (ISession utilitiesSession = UtilitySessionFactory.OpenSession())
		//        {
		//            using (ITransaction transaction = utilitiesSession.BeginTransaction())
		//            {
		//                utilitiesSession.Save(step);
		//                transaction.Commit();
		//            }
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        Log.Error(ex.Message, ex);
		//        throw;
		//    }
		//}

	}


}
