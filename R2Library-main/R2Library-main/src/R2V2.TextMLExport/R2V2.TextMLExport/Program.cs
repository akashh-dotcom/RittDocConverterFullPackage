using System;
using System.Collections;
using System.Configuration;
using System.Reflection;
using R2Library.Data.ADO.Config;
using R2V2.TextMLExport.Tasks;
using R2V2.TextMLExport.Tasks.TextML;
using log4net;

namespace R2V2.TextMLExport
{
	public class Program
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

		public static void Main(string[] args)
		{
			log4net.Config.XmlConfigurator.Configure();
			Log.Debug("Main() >>>");

			//DbConfigSettings.Settings = new R2UtilitiesDbConfigSettings();
			//Log.DebugFormat("R2DatabaseConnection: {0}", R2UtilitiesSetting.Default.R2DatabaseConnection);

			// list properties
			SortedList sortedProperties = new SortedList();
			foreach (SettingsProperty property in Setting.Default.Properties)
			{
				sortedProperties.Add(property.Name, property.DefaultValue.ToString());
			}
			foreach (var key in sortedProperties.Keys)
			{
				if (sortedProperties[key].ToString() ==
				    Setting.Default.GetType().GetProperty(key).GetValue(Setting.Default, null).ToString())
				{
					Log.DebugFormat("TaskName: {0}, DefaultValue: {1}", key, sortedProperties[key]);
				}
				else
				{
					Log.WarnFormat("TaskName: {0}, DefaultValue: {1}, Value: {2}", key, sortedProperties[key],
					               Setting.Default.GetType().GetProperty(key).GetValue(Setting.Default, null));
				}
			}

			DbConfigSettings.Settings = new R2DbConfigSettings(Setting.Default.R2DatabaseConnection, Setting.Default.R2UtilitiesDatabaseConnection);

			Console.WriteLine("");
			Console.WriteLine("R2v2 TextML Export");

			try
			{
				string arg;
				if (args.Length == 0)
				{
					Console.WriteLine();
					Console.WriteLine("01 = ExtractTexMLContent");
					Console.WriteLine();
					Console.Write("Please enter code: ");
					arg = Console.ReadLine();
				}
				else
				{
					arg = args[0];
				}


				Log.Info("*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-");
				Log.InfoFormat("arg: {0}", arg);
				Log.Info("*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-");
				Console.WriteLine();

				ITask task = null;
				switch (arg)
				{
					case "-ExtractTexMLContent":
					case "01":
						task = new TextMLExtractTask();
						break;

					default:
						Console.WriteLine("INVALID AURGUMENT");
						break;
				}

				if (task != null)
				{
					try
					{
						task.Run();
						task.TaskResult.CompletedSuccessfully = true;
						task.TaskResult.RunComments = "Successfully Completed.";

						//foreach (TaskResultStep step in task.TaskResult.Steps)
						//{
						//    if (!step.TaskCompletedSuccessfully)
						//    {
						//        task.TaskResult.TaskCompletedSuccessfully = false;
						//        task.TaskResult.TaskResults = string.Format("Step {0} failed.", step.Id);
						//    }
						//}
					}
					catch (Exception ex)
					{
						task.TaskResult.RunComments = string.Format("EXCEPTION: {0}", ex.Message);
						task.TaskResult.CompletedSuccessfully = false;
						Log.Error(ex.Message, ex);
					}
					finally
					{
						task.Cleanup();
					}

				}
			}
			catch (Exception ex)
			{
				Log.ErrorFormat(ex.Message, ex);
			}

			Console.WriteLine();
			Log.Debug("Main() <<<");
		}
	}
}
