#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2Utilities.Tasks.MaintenanceTasks
{
    public class UpdateWithOnixDataTask : TaskBase
    {
        private readonly ILog<UpdateWithOnixDataTask> _log;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly ResourceCoreDataService _resourceCoreDataService;
        private readonly IQueryable<Resource> _resources;

        public UpdateWithOnixDataTask(
            IR2UtilitiesSettings r2UtilitiesSettings
            , IQueryable<Resource> resources
            , ResourceCoreDataService resourceCoreDataService
            , ILog<UpdateWithOnixDataTask> log)
            : base("UpdateWithOnixDataTask", "-UpdateWithOnixDataTask", "18", TaskGroup.ContentLoading,
                "Updates R2library Resources from ONIX data", true)
        {
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _resources = resources;
            _resourceCoreDataService = resourceCoreDataService;
            _log = log;
        }


        public override void Run()
        {
            TaskResult.Information = "This task will update/insert eIsbns from Onix.";
            var step = new TaskResultStep { Name = "UpdateWithOnixDataTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            var resultsBuilder = new StringBuilder();

            try
            {
                var eIsbnsFound = new List<OnixEisbn>();

                var resources = _resources.ToList();
                var isbn13List = resources.Select(x => x.Isbn13).ToArray();

                var takeCount = _r2UtilitiesSettings.EIsbnGetRequestCount;
                var totalCount = isbn13List.Length;

                var i = 0;
                while (true)
                {
                    var isbns = isbn13List.Skip(i * takeCount).Take(takeCount).ToArray();

                    if (isbns.Length == 0)
                    {
                        break;
                    }

                    _log.InfoFormat("Checking Isbns {0}/{1}", isbns.Length + i * takeCount, totalCount);

                    var onixEisbns = RequestOnixEisbns(_r2UtilitiesSettings.EIsbnGetUrl, isbns);

                    _log.InfoFormat("Found eIsbns {0}/{1}", onixEisbns.Count, isbns.Length);

                    eIsbnsFound.AddRange(onixEisbns);
                    i++;
                }

                resultsBuilder.AppendFormat("{0} eIsbns found. ", eIsbnsFound.Count).AppendLine();

                if (eIsbnsFound.Any())
                {
                    //Filter out all eISBNs that are already in the database
                    eIsbnsFound = FilterNewEisbns(eIsbnsFound, resources);

                    resultsBuilder.AppendFormat("{0} eIsbns to Update/Insert. ", eIsbnsFound.Count).AppendLine();

                    var onixEisbnDuplidates = GetOverLappingIsbns(eIsbnsFound, resources);

                    var dups = onixEisbnDuplidates.Select(y => y.OnixEisbn);

                    var eIsbnsToUpdate = eIsbnsFound.Where(x => !dups.Contains(x)).ToList();

                    var numberUpdateInsert = _resourceCoreDataService.UpdateEisbns(eIsbnsToUpdate);

                    resultsBuilder
                        .AppendFormat("{0} eIsbns updated/inserted.",
                            numberUpdateInsert > 1 ? numberUpdateInsert / 2 : 0).AppendLine().AppendLine();

                    if (onixEisbnDuplidates.Any())
                    {
                        resultsBuilder.Append(" Cannot update the following eISBN because of overlaps.").AppendLine();
                        foreach (var onixEisbnDuplidate in onixEisbnDuplidates)
                        {
                            var onixItem = onixEisbnDuplidate.OnixEisbn;
                            resultsBuilder.AppendLine().AppendFormat("Skipped: Isbn10:{0} Isbn13-{1} new eIsbn-{2}",
                                onixItem.Isbn10, onixItem.Isbn13, onixItem.EIsbn13).AppendLine();

                            foreach (var item in onixEisbnDuplidate.DuplicateResoruces)
                            {
                                resultsBuilder
                                    .AppendFormat("ResourceId: {0} Isbn10: {1} Isbn13: {2} eIsbn: {3} Title: {4}",
                                        item.Id, item.Isbn10, item.Isbn13, item.EIsbn, item.Title).AppendLine();
                            }
                        }
                    }
                }

                step.Results = resultsBuilder.AppendLine().ToString();
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

        public List<OnixEisbn> FilterNewEisbns(List<OnixEisbn> eIsbnsFound, List<Resource> resources)
        {
            var eIsbnsToUpdate = new List<OnixEisbn>();
            foreach (var onixEisbn in eIsbnsFound)
            {
                var resource =
                    resources.FirstOrDefault(x => x.Isbn13 == onixEisbn.Isbn13 && x.EIsbn == onixEisbn.EIsbn13);
                if (resource == null)
                {
                    eIsbnsToUpdate.Add(onixEisbn);
                }
            }

            return eIsbnsToUpdate;
        }

        public List<OnixEisbnDuplidate> GetOverLappingIsbns(List<OnixEisbn> eIsbnsFound, List<Resource> resources)
        {
            var dups = new List<OnixEisbnDuplidate>();
            foreach (var onixEisbn in eIsbnsFound)
            {
                var resourcesFound = resources.Where(x =>
                        x.Isbn == onixEisbn.EIsbn13 || x.Isbn13 == onixEisbn.EIsbn13 || x.EIsbn == onixEisbn.EIsbn13)
                    .ToList();
                if (resourcesFound.Any())
                {
                    dups.Add(new OnixEisbnDuplidate(onixEisbn, resourcesFound));
                }
            }

            return dups;
        }


        public List<OnixEisbn> RequestOnixEisbns(string onixEisbnServiceEndpoint, string[] isbns)
        {
            _log.Info("Requesting ONIX eIsbns...");
            if (isbns == null)
            {
                return null;
            }

            using (var client = new WebClient())
            {
                var nameValueCollection = new NameValueCollection();

                for (var i = 0; i < isbns.Length; i++)
                {
                    nameValueCollection.Add($"isbns[{i}]", isbns[i]);
                }

                client.QueryString = nameValueCollection;

                var jsonResult = client.DownloadString(onixEisbnServiceEndpoint);

                var onixCoverImages = JsonConvert.DeserializeObject<IEnumerable<OnixEisbn>>(jsonResult);

                return onixCoverImages.ToList();
            }
        }
    }
}