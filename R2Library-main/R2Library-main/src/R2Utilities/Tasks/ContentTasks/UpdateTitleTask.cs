#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2Utilities.Tasks.ContentTasks.Services;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class UpdateTitleTask : TaskBase, ITask
    {
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;

        private readonly ResourceCoreDataService _resourceCoreDataService;
        private readonly TitleXmlService _titleXmlService;
        private string _file;
        private bool _ignoreEqual;
        private Dictionary<string, string> _isbnAndTitles;

        private bool _isTestMode;
        private int _maxResourceId;
        private int _maxResources;
        private int _minResourceId;
        private bool _revertAll;

        /// <summary>
        ///     -UpdateTitleTask -testmode=true -revertAll=true -ignoreEqual=true -file= -maxResources=500 -minResourceId=7000
        ///     -maxResourceId=8000
        /// </summary>
        public UpdateTitleTask(
            ResourceCoreDataService resourceCoreDataService
            , IR2UtilitiesSettings r2UtilitiesSettings
            , TitleXmlService titleXmlService
        )
            : base(
                "UpdateTitleTask", "-UpdateTitleTask", "19", TaskGroup.ContentLoading,
                "Replaces the title in the XML files with the Rittenhouse Title.", true)
        {
            _resourceCoreDataService = resourceCoreDataService;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _titleXmlService = titleXmlService;
        }

        public new void Init(string[] commandLineArguments)
        {
            base.Init(commandLineArguments);

            // -testmode=true -revertAll=true -ignoreEqual=true -file= -maxResources=500 -minResourceId=7000 -maxResourceId=8000
            _isTestMode = GetArgumentBoolean("testmode", true);
            _revertAll = GetArgumentBoolean("revertAll", true);
            _ignoreEqual = GetArgumentBoolean("ignoreEqual", true);

            _file = GetArgument("file");

            _maxResources = GetArgumentInt32("maxResources", 100000);
            _minResourceId = GetArgumentInt32("minResourceId", 0);
            _maxResourceId = GetArgumentInt32("maxResourceId", 100000);

            Log.InfoFormat(">>> _isTestMode: {0}, _revertAll: {1}, _resourceFileTableName: {2}", _isTestMode,
                _revertAll, _file);
            Log.InfoFormat(">>> _minResourceId: {0}, _maxResourceId: {1}, _maxResources: {2}", _minResourceId,
                _maxResourceId, _maxResources);

            if (CommandLineArguments.Length > 1)
            {
                var args = CommandLineArguments;

                string isbn = null;
                _isbnAndTitles = new Dictionary<string, string>();
                foreach (var commandLineArgument in args)
                {
                    if (commandLineArgument.Contains("-isbn="))
                    {
                        var arg = commandLineArgument.Replace("-isbn=", "");
                        isbn = arg;
                    }
                    else if (commandLineArgument.Contains("-title="))
                    {
                        var arg = commandLineArgument.Replace("-title=", "");
                        var title = arg;
                        if (!string.IsNullOrWhiteSpace(isbn))
                        {
                            _isbnAndTitles.Add(isbn, title);
                        }
                    }
                }

                if (!_isbnAndTitles.Any())
                {
                    _isbnAndTitles = null;
                }
            }
        }

        public override void Run()
        {
            TaskResult.Information = TaskDescription;
            var step = new TaskResultStep { Name = "UpdateTitleTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            try
            {
                var rittenhouseResourceTitles = GetRittenhouseResourceTitles();

                //Will revery all XML and do nothing else
                if (_revertAll)
                {
                    var restoredResources = _titleXmlService.RestoreXmlFiles(rittenhouseResourceTitles, _isTestMode);
                    step.Results = $"{restoredResources} Resources have had there XML reverted to the backup.";
                }
                else
                {
                    var resourcesUpdated = 0;

                    var resultFlatFile = StartResultFile();

                    foreach (var item in rittenhouseResourceTitles)
                    {
                        var lastStep = _titleXmlService.UpdateTitleXml(item, TaskResult, _isTestMode);
                        resourcesUpdated++;
                        AppendResultToFile(resultFlatFile, item, lastStep);
                    }

                    step.Results = $"{resourcesUpdated} Resource Titles Updated";
                }

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

        private string StartResultFile()
        {
            var resultFlatFile = Path.Combine(_r2UtilitiesSettings.UpdateTitleTaskWorkingFolder,
                $"UpdateTitleTask_Result_{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt");

            var file = new StreamWriter(resultFlatFile);
            file.WriteLine("ResourceId\tISBN\tNew Title\tOld Title\tComplete Status\tDescription");
            file.Close();
            return resultFlatFile;
        }

        private void AppendResultToFile(string fileName, ResourceTitleChange item, TaskResultStep step)
        {
            using (var sw = File.AppendText(fileName))
            {
                sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", item.ResourceId, item.Isbn, item.GetNewTitle(), item.Title,
                    step.CompletedSuccessfully ? "Success" : "Fail", step.Results.Replace("\r\n", " "));
            }
        }

        private List<ResourceTitleChange> GetRittenhouseResourceTitles()
        {
            var rittenhouseResourceTitles = ParseArgumentTitles();
            if (rittenhouseResourceTitles == null && _file == null && _isbnAndTitles == null)
            {
                rittenhouseResourceTitles = new List<ResourceTitleChange>();

                var rittenhouseResourceTitlesFromDatabase =
                    _resourceCoreDataService.GetRittenhouseTitles(_r2UtilitiesSettings.PreludeDataLinkedServer,
                        _minResourceId, _maxResourceId);

                var dictionary = new Dictionary<ResourceTitleUpdateType, List<ResourceTitleChange>>
                {
                    {
                        ResourceTitleUpdateType.Equal,
                        rittenhouseResourceTitlesFromDatabase.Where(x => x.UpdateType == ResourceTitleUpdateType.Equal)
                            .ToList()
                    },
                    {
                        ResourceTitleUpdateType.RittenhouseEqualR2TitleAndSub,
                        rittenhouseResourceTitlesFromDatabase.Where(x =>
                            x.UpdateType == ResourceTitleUpdateType.RittenhouseEqualR2TitleAndSub).ToList()
                    },
                    {
                        ResourceTitleUpdateType.R2EqualRittenhouseTitleAndSub,
                        rittenhouseResourceTitlesFromDatabase.Where(x =>
                            x.UpdateType == ResourceTitleUpdateType.R2EqualRittenhouseTitleAndSub).ToList()
                    },
                    {
                        ResourceTitleUpdateType.NotExist,
                        rittenhouseResourceTitlesFromDatabase
                            .Where(x => x.UpdateType == ResourceTitleUpdateType.NotExist).ToList()
                    },
                    {
                        ResourceTitleUpdateType.DifferentSub,
                        rittenhouseResourceTitlesFromDatabase
                            .Where(x => x.UpdateType == ResourceTitleUpdateType.DifferentSub)
                            .ToList()
                    },
                    {
                        ResourceTitleUpdateType.RittenhouseSubNull,
                        rittenhouseResourceTitlesFromDatabase
                            .Where(x => x.UpdateType == ResourceTitleUpdateType.RittenhouseSubNull)
                            .ToList()
                    },
                    {
                        ResourceTitleUpdateType.R2SubNull,
                        rittenhouseResourceTitlesFromDatabase
                            .Where(x => x.UpdateType == ResourceTitleUpdateType.R2SubNull).ToList()
                    },
                    {
                        ResourceTitleUpdateType.Other,
                        rittenhouseResourceTitlesFromDatabase.Where(x => x.UpdateType == ResourceTitleUpdateType.Other)
                            .ToList()
                    }
                };

                foreach (var keyValuePair in dictionary)
                {
                    Log.Info($"{keyValuePair.Key}-Count:{keyValuePair.Value.Count}");
                }

                var titleTypeToIgnore = new[] { ResourceTitleUpdateType.NotExist, ResourceTitleUpdateType.Other };

                foreach (var keyValuePair in dictionary)
                {
                    if (!titleTypeToIgnore.Contains(keyValuePair.Key))
                    {
                        rittenhouseResourceTitles.AddRange(keyValuePair.Value);
                    }
                }
            }

            if (rittenhouseResourceTitles == null)
            {
                return null;
            }

            if (_ignoreEqual)
            {
                rittenhouseResourceTitles = rittenhouseResourceTitles.Where(x => x.Title != x.GetNewTitle()).ToList();
            }

            if (rittenhouseResourceTitles.Count > _maxResources)
            {
                rittenhouseResourceTitles = rittenhouseResourceTitles.GetRange(0, _maxResources);
            }

            foreach (var rittenhouseResourceTitle in rittenhouseResourceTitles)
            {
                Log.Info($"{rittenhouseResourceTitle.Isbn} -- to be Updated");
            }

            return rittenhouseResourceTitles;
        }

        private List<ResourceTitleChange> ParseArgumentTitles()
        {
            List<ResourceTitleChange> rittenhouseResourceTitles = null;
            if (!string.IsNullOrWhiteSpace(_file))
            {
                var file = new FileInfo(_file);
                if (file.Exists)
                {
                    var js = new JavaScriptSerializer();
                    var titleUpdateProducts = new List<TitleUpdateProduct>();

                    using (TextReader tr = file.OpenText())
                    {
                        var line = tr.ReadLine();

                        while (!string.IsNullOrWhiteSpace(line))
                        {
                            var titleUpdateProduct =
                                (TitleUpdateProduct)js.Deserialize(line, typeof(TitleUpdateProduct));
                            titleUpdateProducts.Add(titleUpdateProduct);
                            line = tr.ReadLine();
                        }
                    }

                    _isbnAndTitles = titleUpdateProducts.ToDictionary(x => x.Isbn, y => y.Title);
                }
            }

            if (_isbnAndTitles != null)
            {
                rittenhouseResourceTitles =
                    _resourceCoreDataService.GetRittenhouseTitles(_r2UtilitiesSettings.PreludeDataLinkedServer,
                        _isbnAndTitles);
            }

            return rittenhouseResourceTitles;
        }
    }

    [Serializable]
    public class TitleUpdateProduct
    {
        public int ResourceId { get; set; }
        public string Isbn { get; set; }
        public string Title { get; set; }
    }
}