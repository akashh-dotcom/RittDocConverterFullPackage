using System;
using R2Library.Data.ADO.R2Utility;

namespace R2Utilities.Tasks.FeedTasks
{
    public class PrecisionSearchFeedTask : EmailTaskBase
    {
        //private readonly FeedTaskService _feedTaskService;
        //private readonly IEmailSettings _emailSettings;
        //private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        //private readonly EmailBuildBaseService _emailMessageBuildService;

        //private string _mainTemplate;
        //private string _filePathAndName;

        public PrecisionSearchFeedTask(
            //FeedTaskService feedTaskService
            //, IEmailSettings emailSettings
            //, IR2UtilitiesSettings r2UtilitiesSettings
            //, EmailBuildBaseService emailMessageBuildService
            )
            : base("PrecisionSearchFeedTask", "-PrecisionSearchFeedTask", "x70", TaskGroup.Deprecated, "Processes Precision Search feeds", false)
        {
            //_feedTaskService = feedTaskService;
            //_emailSettings = emailSettings;
            //_r2UtilitiesSettings = r2UtilitiesSettings;
            //_emailMessageBuildService = emailMessageBuildService;
        }

        public override void Run()
        {
            TaskResult.Information = "Precision Search Feed Task";
            var step = new TaskResultStep {Name = "PrecisionSearchTask", StartTime = DateTime.Now};
            TaskResult.AddStep(step);
            UpdateTaskResult();
            try
            {
                //_filePathAndName = string.Format("{0}\\PrecisionSearchSubscriptions-{1}-{2:00}-{3:00}.csv",
                //    _r2UtilitiesSettings.GenericFilePath
                //    , DateTime.Now.Date.ToShortDateString().Replace('/', '-')
                //    , DateTime.Now.Hour, DateTime.Now.Minute
                //    );


                //List<PrecisionSearchSubscription> subscriptions = _feedTaskService.GetPrecisionSearchCustomers();

                //bool success = ProcessPrecisionSubscriptions(subscriptions);

                step.CompletedSuccessfully = true;
                step.Results = string.Format("Precision Search is no longer a feature.");

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

        //    /// <summary>
        //    /// Processes the subscriptions including writing to file, building email, and sending email.
        //    /// </summary>
        //        //        //    public bool ProcessPrecisionSubscriptions(IEnumerable<PrecisionSearchSubscription> subscriptions)
        //    {
        //            _mainTemplate = File.ReadAllText(string.Format("{0}Main_Header_Footer.html", _emailSettings.TemplatesDirectory));
        //            var mainBuilder = new StringBuilder()
        //                .Append(_mainTemplate
        //                            .Replace("{Title}", "Precision Search Accounts and Statues")
        //                            .Replace("{Body}", "<p>The attachment contains all the R2library accounts that have subscriptions to Precision Search.</p>")
        //                            .Replace("{Year}", DateTime.Now.Year.ToString())
        //                            .Replace("{WebsiteUrl}", _emailMessageBuildService.GetWebSiteBaseUrl())
        //                            .Replace("{User_Email}", "")
        //                            .Replace("{Institution_Name}", "")
        //                            .Replace("(#{Institution_Number}) -", "")).ToString();

        //        var fileArray = _filePathAndName.Split('\\');
        //        var fileName = fileArray.Last();

        //        var contentType = new ContentType { Name = fileName };

        //        WritePrecisionSearchToFile(subscriptions);
        //        var attachment = new Attachment(_filePathAndName, contentType) { ContentType = { MediaType = "text/csv" } };

        //        var emails = _r2UtilitiesSettings.PrecisionSearchEmailArray.Split(';');

        //        return SendEmailTaskEmail(mainBuilder, emails, attachment);
        //    }

        //    /// <summary>
        //    /// Writes the Subscriptions to file
        //    /// </summary>
        //        //    public void WritePrecisionSearchToFile(IEnumerable<PrecisionSearchSubscription> subscriptions)
        //    {
        //        using (StreamWriter file = new StreamWriter(_filePathAndName))
        //        {
        //            var sb = new StringBuilder()
        //                .Append("\"SourceCode\",\"AccountID\",\"CustomerName\",\"CustomerAddress1\",\"CustomerAddress2\",\"CustomerCity\",\"CustomerState\"")
        //                .Append(",\"CustomerPostalCode\",\"CustomerCountry\",\"CustomerPhone\",\"CustomerAdminContactLastName\",\"CustomerAdminContactFirstName\"")
        //                .Append(",\"CustomerAdminContactEmailAddress\",\"CustomerAdminContactAddress1\",\"CustomerAdminContactAddress2\",\"CustomerAdminContactCity\"")
        //                .Append(",\"CustomerAdminContactState\",\"CustomerAdminContactPostalCode\",\"CustomerAdminContactCountry\",\"CustomerAdminContactPhone\"")
        //                .Append(",\"CustomerAdminContactJobTitle\",\"CustomerOrganizationType\",\"SuscriptionStatus\",\"StartDate\",\"EndDate\"");

        //            file.WriteLine(sb);

        //            foreach (var item in subscriptions)
        //            {
        //                var subscription = new StringBuilder()
        //                    .AppendFormat("\"{0}", item.SourceCode)
        //                    .AppendFormat("\",\"{0}", item.AccountId)
        //                    .AppendFormat("\",\"{0}", item.Name)
        //                    .AppendFormat("\",\"{0}", item.Address.Address1)
        //                    .AppendFormat("\",\"{0}", item.Address.Address2)
        //                    .AppendFormat("\",\"{0}", item.Address.City)
        //                    .AppendFormat("\",\"{0}", item.Address.State)
        //                    .AppendFormat("\",\"{0}", item.Address.Zip)
        //                    .AppendFormat("\",\"{0}", "USA")
        //                    .AppendFormat("\",\"{0}", item.Phone)
        //                    .AppendFormat("\",\"{0}", item.PrecisionSearchAdminContact.LastName)
        //                    .AppendFormat("\",\"{0}", item.PrecisionSearchAdminContact.FirstName)
        //                    .AppendFormat("\",\"{0}", item.PrecisionSearchAdminContact.EmailAddress)
        //                    .AppendFormat("\",\"{0}", item.PrecisionSearchAdminContact.Address.Address1)
        //                    .AppendFormat("\",\"{0}", item.PrecisionSearchAdminContact.Address.Address2)
        //                    .AppendFormat("\",\"{0}", item.PrecisionSearchAdminContact.Address.City)
        //                    .AppendFormat("\",\"{0}", item.PrecisionSearchAdminContact.Address.State)
        //                    .AppendFormat("\",\"{0}", item.PrecisionSearchAdminContact.Address.Zip)
        //                    .AppendFormat("\",\"{0}", "USA")
        //                    .AppendFormat("\",\"{0}", item.PrecisionSearchAdminContact.Phone)
        //                    .AppendFormat("\",\"{0}", item.PrecisionSearchAdminContact.JobTitle)
        //                    .AppendFormat("\",\"{0}", item.OrganizationType)
        //                    .AppendFormat("\",\"{0}", item.Status)
        //                    .AppendFormat("\",\"{0}", item.StateDate)
        //                    .AppendFormat("\",\"{0}\"", item.EndDate)
        //                    .ToString();
        //                file.WriteLine(subscription);
        //            }
        //        }
        //    }
        //}
    }
}
