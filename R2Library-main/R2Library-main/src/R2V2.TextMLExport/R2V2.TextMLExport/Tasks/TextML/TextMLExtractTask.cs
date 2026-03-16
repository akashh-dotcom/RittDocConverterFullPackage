using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using R2Library.Data.ADO.R2Utility;
using R2Library.Data.ADO.R2Utility.DataServices;
using log4net;

namespace R2V2.TextMLExport.Tasks.TextML
{
	public class TextMLExtractTask : TaskBase
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

		public TextMLExtractTask() : base("ExtractTextMLContent")
		{

		}

		public override void Run()
		{
			try
			{
				// get list of resources to extract
				TaskResultStep step1 = new TaskResultStep { Name = "ExtractResources", StartTime = DateTime.Now };
				TaskResult.AddStep(step1);
				UpdateTaskResult();

				ExtractQueueDataService extractQueueDataService = new ExtractQueueDataService();
				Log.InfoFormat("Batch Size: {0}", Setting.Default.BatchSize);
				Log.InfoFormat("Queue Size: {0}", extractQueueDataService.GetQueueSize());
				List<ExtractQueue> extractQueues = extractQueueDataService.GetNextBatch(Setting.Default.BatchSize).ToList();

				TextMLService textMlService = new TextMLService();
				TransformQueueDataService transformQueueDataService = new TransformQueueDataService();

				int extractedCount = 0;
				foreach (ExtractQueue extractQueue in extractQueues)
				{
					Log.Info(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
					Log.InfoFormat("Processing {0} of {1} - ISBN: {2}, resource id: {3}", (extractedCount + 1), extractQueues.Count, extractQueue.Isbn, extractQueue.ResourceId);

					try
					{
						extractQueue.DateStarted = DateTime.Now;
						int docCount = textMlService.ExtractResourceDocuments(extractQueue.Isbn);
						Log.InfoFormat("docCount: {0}", docCount);

						bool extractedBookXml = textMlService.ExtractBookXmlFileFromTextML(extractQueue.Isbn);
						extractQueue.Status = "X";
						extractQueue.StatusMessage = string.Format("{0} resources extracted, book XML extracted: {1}", docCount, extractedBookXml);
					}
					catch (Exception ex)
					{
						Log.WarnFormat(ex.Message, ex);
						extractQueue.Status = "E";
						extractQueue.StatusMessage = string.Format("Exception: {0}", ex.Message);
					}

					extractQueue.DateFinished = DateTime.Now;

					extractQueueDataService.Update(extractQueue);

					if (extractQueue.Status != "E")
					{
						transformQueueDataService.Insert(extractQueue.ResourceId, extractQueue.Isbn, "A");
					}

					Log.Info("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
					extractedCount++;
				}


				//TextMLService textMlService = new TextMLService();
				//textMlService.ExtractAllResourcesDocuments();

				//step1.CompletedSuccessfully = true;
				//step1.EndTime = DateTime.Now;
				//UpdateTaskResult();

				//// get list of resources from RIT001 to extract from TextML
				//TaskResultStep step2 = new TaskResultStep { Name = "GetResources", StartTime = DateTime.Now };
				//TaskResult.AddStep(step2);
				//UpdateTaskResult();
				////IList<Resource> resources = GetResources();
				//step2.CompletedSuccessfully = true;
				//step2.EndTime = DateTime.Now;
				//UpdateTaskResult();

				//TaskResultStep step3 = new TaskResultStep { Name = "GetResources", StartTime = DateTime.Now };
				//TaskResult.AddStep(step3);
				//UpdateTaskResult();





				//int resourceCount = 0;
				//int resourceCountTotal = resources.Count;
				//foreach (Resource resource in resources)
				//{
				//    resourceCount++;
				//    Log.Info(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
				//    Log.InfoFormat("Resource Id: {0}, ISBN: {1}", resource.Id, resource.Isbn);
				//    Log.InfoFormat("Title: {0}", resource.Title);
				//    Log.InfoFormat("StatusId: {0}, RecordStatus: {1}", resource.StatusId, resource.RecordStatus);
				//    Log.InfoFormat("Processing resource {0} of {1}", resourceCount, resourceCountTotal);

				//    if (resource.StatusId == 8)
				//    {
				//        Log.Info("RESOURCE IGNORED, RESOURCE IS FORTHCOMING");
				//        Log.Info("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
				//        continue;
				//    }

				//    if (resource.StatusId == 72)
				//    {
				//        Log.Info("RESOURCE IGNORED, RESOURCE IS INACTIVE");
				//        Log.Info("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
				//        continue;
				//    }

				//    if (WasResourceAlreadyProcessed(resource, extractedResources))
				//    {
				//        Log.Info("resource already processed");
				//        Log.Info("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
				//        continue;
				//    }

				//    ExtractedResource extractedResource = new ExtractedResource();
				//    extractedResource.ResourceId = resource.Id;
				//    extractedResource.Isbn = resource.Isbn;

				//    try
				//    {
				//        int docCount = TextMLService.ExtractResourceDocuments(resource.Isbn);
				//        extractedResource.DateCompleted = DateTime.Now;
				//        extractedResource.Successfully = true;
				//        extractedResource.Results = string.Format("{0} documents extracted from TextML.", docCount);
				//        Log.InfoFormat("docCount: {0}", docCount);
				//    }
				//    catch (Exception ex)
				//    {
				//        Log.WarnFormat(ex.Message, ex);
				//        extractedResource.DateCompleted = DateTime.Now;
				//        extractedResource.Successfully = false;
				//        extractedResource.Results = ex.Message;
				//    }
				//    UpdateExtractedResource(extractedResource);

				//    textMlService.GetBookXmlFileFromTextML(resource.Isbn);

				//    Log.Info("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
				//}
				//step3.StepCompletedSuccessfully = true;
				//step3.StepEndTime = DateTime.Now;
				UpdateTaskResult();
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message, ex);
				throw;
			}
		}

		///// <summary>
		/////
		///// </summary>
		///// <returns></returns>
		//private IList<ExtractedResource> GetExtractedResources()
		//{
		//    Log.Debug(">>>>>>>>>>>>");
		//    using (ISession utilitiesSession = UtilitySessionFactory.OpenSession())
		//    {
		//        var results = utilitiesSession.QueryOver<ExtractedResource>()
		//            .Where(x => x.Successfully == true)
		//            .Future()
		//            .OrderBy(x => x.ResourceId);

		//        return results.ToList();
		//    }
		//}

		///// <summary>
		/////
		///// </summary>
		///// <returns></returns>
		//private IList<Resource> GetResources()
		//{
		//    using (ISession r2Session = R2SessionFactory.OpenSession())
		//    {
		//        Log.Debug(">>>>>>>>>>>>");
		//        var results = r2Session.QueryOver<Resource>()
		//            //.Where(r => r.Isbn == "0915473623")// or r.Isbn = "0915473623"))
		//            .Future()
		//            .OrderBy(x => x.Id);

		//        return results.ToList();
		//    }
		//}

		///// <summary>
		/////
		///// </summary>
		///// <param name="extractedResource"></param>
		//protected void UpdateExtractedResource(ExtractedResource extractedResource)
		//{
		//    try
		//    {
		//        using (ISession utilitiesSession = UtilitySessionFactory.OpenSession())
		//        {
		//            using (ITransaction transaction = utilitiesSession.BeginTransaction())
		//            {
		//                utilitiesSession.Save(extractedResource);
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

		///// <summary>
		/////
		///// </summary>
		///// <param name="resource"></param>
		///// <param name="extractedResources"></param>
		///// <returns></returns>
		//private bool WasResourceAlreadyProcessed(Resource resource, IEnumerable<ExtractedResource> extractedResources)
		//{
		//    return extractedResources.Any(extractedResource => resource.Id == extractedResource.ResourceId);
		//}

		///// <summary>
		/////
		///// </summary>
		///// <param name="isbn"></param>
		//private void TestBookXml(string isbn)
		//{
		//    TextMLService textMlService = new TextMLService();
		//    textMlService.GetBookXmlFileFromTextML(isbn);
		//}
	}
}
