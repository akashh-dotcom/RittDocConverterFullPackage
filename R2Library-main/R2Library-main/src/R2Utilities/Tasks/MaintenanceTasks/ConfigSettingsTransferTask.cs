#region

using System;
using System.IO;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2V2.Core.Configuration;

#endregion

namespace R2Utilities.Tasks.MaintenanceTasks
{
    /// <summary>
    ///     <para>-ConfigSettingsTransfer -file=F:\Clients\Rittenhouse\Temp\configSettings_{date}.sql</para>
    /// </summary>
    public class ConfigSettingsTransferTask : TaskBase, ITask
    {
        private readonly IQueryable<DbConfigurationSetting> _configurationSettings;

        private string _file;

        public ConfigSettingsTransferTask(
            IQueryable<DbConfigurationSetting> configurationSettings
        )
            : base("ConfigSettingsTransfer", "-ConfigSettingsTransfer", "12", TaskGroup.ContentLoading,
                "Task to export or import Configuration Settings", true)
        {
            _configurationSettings = configurationSettings;
        }

        public new void Init(string[] commandLineArguments)
        {
            base.Init(commandLineArguments);

            var file = GetArgument("file");
            if (file.Contains("{"))
            {
                file = file.Replace("date", "0");
                _file = string.Format(file, $"_{DateTime.Now:yyyyMdd-HHmmss}");
            }
            else
            {
                _file = file;
            }

            Log.Info($"-job: ConfigSettingsTransferTask, -file: {_file}");
        }

        public override void Run()
        {
            TaskResult.Information = new StringBuilder()
                .Append("This task will export or import the Configuration Settings from the RIT001 Database. ")
                .Append(
                    "The data is written to a json file and the file will only be saved if the data is different than the previous file. ")
                .ToString();

            var step = new TaskResultStep { Name = "ConfigSettingsTransfer", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                var resultsBuilder = new StringBuilder();
                var success = ExportFile(resultsBuilder);
                resultsBuilder.Append($"Export succeeded : {success}");
                step.Results = resultsBuilder.ToString();
                step.CompletedSuccessfully = success;
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

        public bool ExportFile(StringBuilder resultsBuilder)
        {
            try
            {
                Log.Info("Start ExportFile()");
                var configurationSettings = _configurationSettings.ToList();
                Log.Info("configurationSettings found");
                using (var sw = new StreamWriter(_file))
                {
                    foreach (var configurationSetting in configurationSettings)
                    {
                        sw.WriteLine(configurationSetting.ToInsertString());
                    }
                }

                Log.Info("file has been written");
                var currentFile = new FileInfo(_file);
                if (currentFile.Exists && currentFile.DirectoryName != null)
                {
                    var searchPattern = currentFile.Name.Contains("_")
                        ? currentFile.Name.Split('_').First()
                        : currentFile.Name.Split('.').First();

                    var directory = new DirectoryInfo(currentFile.DirectoryName);

                    var latestFile = (from f in directory.GetFiles($"{searchPattern}*")
                            where f.Name != currentFile.Name
                            orderby f.LastWriteTime descending
                            select f
                        ).FirstOrDefault();
                    if (latestFile != null)
                    {
                        var same = File.ReadLines(currentFile.FullName)
                            .SequenceEqual(File.ReadLines(latestFile.FullName));
                        if (same)
                        {
                            Log.Info("Configurations have NOT changed. Deleting file that was just created.");
                            File.Delete(currentFile.FullName);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                resultsBuilder.AppendLine($"{ex.Message}\r\n");
            }

            return false;
        }
    }
}