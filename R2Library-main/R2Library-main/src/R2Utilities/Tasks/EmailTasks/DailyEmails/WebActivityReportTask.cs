#region

using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess.WebActivity;
using R2Utilities.Email.EmailBuilders;
using R2Utilities.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.EmailTasks.DailyEmails
{
    public class WebActivityReportTask : EmailTaskBase
    {
        private readonly InternalUtilitiesEmailBuildService _emailBuildService;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly WebActivityService _webActivityService;

        private DateTime _startDate;

        public WebActivityReportTask(
            IR2UtilitiesSettings r2UtilitiesSettings
            , WebActivityService webActivityService
            , InternalUtilitiesEmailBuildService emailBuildService
        )
            : base("WebActivityReportTask", "-WebActivityReportTask", "62", TaskGroup.InternalSystemEmails,
                "Sends the web activity report email", true)
        {
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _webActivityService = webActivityService;
            _emailBuildService = emailBuildService;
        }

        public override void Run()
        {
            _startDate = DateTime.Now.Date;
            //Testing Only
            //_startDate = DateTime.Parse("5/02/2015 0:0:01");

            TaskResult.Information = "Web Activity Report Task Run";
            var step = new TaskResultStep { Name = "WebActivityReportTask Run", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                var webActivityReport = GetWebActivityReport();

                var success = ProcessWebActivityReport(webActivityReport);

                step.CompletedSuccessfully = success;
                step.Results = "Web Activity Report Task completed successfully";
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

        private bool ProcessWebActivityReport(WebActivityReport webActivityReport)
        {
            TaskResult.Information = "Process Web Activity Report";
            var step = new TaskResultStep { Name = "ProcessWebActivityReport (Build Email)", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            //_emailBuildService.SetTemplates(EmailTemplates.WebActivityBody, EmailTemplates.WebActivityItem, false, EmailTemplates.WebActivitySubItem);
            _emailBuildService.InitEmailTemplates();

            try
            {
                var itemBuilder = new StringBuilder();

                if (webActivityReport.TopInstitutionPageRequests != null)
                {
                    var resultBuilder = new StringBuilder();
                    foreach (var topInstitution in webActivityReport.TopInstitutionPageRequests)
                    {
                        resultBuilder.Append(_emailBuildService.BuildWebActivitySubItemHtml(topInstitution.Count,
                            GetResultDetails(topInstitution)));
                    }

                    itemBuilder.Append(_emailBuildService.BuildWebActivityItemHtml(
                        "<span style=\"color:0033FF;\" >Top Institution Page Requests</span>", resultBuilder.ToString(),
                        "color:#0033FF;"));
                }

                if (webActivityReport.TopResources != null)
                {
                    var resultBuilder = new StringBuilder();
                    foreach (var topResource in webActivityReport.TopResources)
                    {
                        resultBuilder.Append(_emailBuildService.BuildWebActivitySubItemHtml(topResource.Count,
                            GetResultDetails(topResource)));
                    }

                    itemBuilder.Append(_emailBuildService.BuildWebActivityItemHtml("Top Resource Requests",
                        resultBuilder.ToString(), "color:#009900;"));
                }

                if (webActivityReport.TopIpRanges != null)
                {
                    var resultBuilder = new StringBuilder();
                    foreach (var topIpRange in webActivityReport.TopIpRanges)
                    {
                        resultBuilder.Append(
                            _emailBuildService.BuildWebActivitySubItemHtml(topIpRange.Count,
                                GetResultDetails(topIpRange)));
                    }

                    itemBuilder.Append(_emailBuildService.BuildWebActivityItemHtml("Top Ip Range Accesses",
                        resultBuilder.ToString(), null));
                }

                if (webActivityReport.TopInstitutionIpRanges != null)
                {
                    var resultBuilder = new StringBuilder();
                    foreach (var topIpRange in webActivityReport.TopInstitutionIpRanges)
                    {
                        resultBuilder.Append(
                            _emailBuildService.BuildWebActivitySubItemHtml(topIpRange.Count,
                                GetResultDetails(topIpRange)));
                    }

                    itemBuilder.Append(_emailBuildService.BuildWebActivityItemHtml("Top Institution Ip Range Accesses",
                        resultBuilder.ToString(), null));
                }

                if (webActivityReport.TopInstitutionResourceRequests != null)
                {
                    var resultBuilder = new StringBuilder();
                    foreach (var topInstitutionsAndResources in webActivityReport.TopInstitutionResourceRequests)
                    {
                        resultBuilder.Append(_emailBuildService.BuildWebActivitySubItemHtml(
                            topInstitutionsAndResources.Count, GetResultDetails(topInstitutionsAndResources)));
                    }

                    itemBuilder.Append(_emailBuildService.BuildWebActivityItemHtml("Top Institution Resource Requests",
                        resultBuilder.ToString(), "color:#009900;"));
                }

                if (webActivityReport.TopInstitutionResourcePrintRequests != null)
                {
                    var resultBuilder = new StringBuilder();
                    foreach (var topInstitutionsAndResources in webActivityReport.TopInstitutionResourcePrintRequests)
                    {
                        resultBuilder.Append(_emailBuildService.BuildWebActivitySubItemHtml(
                            topInstitutionsAndResources.Count, GetResultDetails(topInstitutionsAndResources)));
                    }

                    itemBuilder.Append(_emailBuildService.BuildWebActivityItemHtml(
                        "Top Institution Resource Print Requests", resultBuilder.ToString(), "color:#330066;"));
                }

                if (webActivityReport.TopInstitutionResourceEmailRequests != null)
                {
                    var resultBuilder = new StringBuilder();
                    foreach (var topInstitutionsAndResources in webActivityReport.TopInstitutionResourceEmailRequests)
                    {
                        resultBuilder.Append(_emailBuildService.BuildWebActivitySubItemHtml(
                            topInstitutionsAndResources.Count, GetResultDetails(topInstitutionsAndResources)));
                    }

                    itemBuilder.Append(_emailBuildService.BuildWebActivityItemHtml(
                        "Top Institution Resource Email Requests", resultBuilder.ToString(), "color:#660000;"));
                }

                if (webActivityReport.TopInstitutionSessionRequests != null)
                {
                    var resultBuilder = new StringBuilder();
                    foreach (var topInstitutionsAndResources in webActivityReport.TopInstitutionSessionRequests)
                    {
                        resultBuilder.Append(_emailBuildService.BuildWebActivitySubItemHtml(
                            topInstitutionsAndResources.Count, GetResultDetails(topInstitutionsAndResources)));
                    }

                    itemBuilder.Append(_emailBuildService.BuildWebActivityItemHtml("Top Institution Sessions",
                        resultBuilder.ToString(), "color:#FF33CC;"));
                }

                var body = _emailBuildService.BuildBody(webActivityReport, itemBuilder, _startDate);

                var emailMessage = _emailBuildService.BuildWebActivityEmail(body,
                    EmailSettings.TaskEmailConfig.ToAddresses.ToArray(), DateTime.Now);

                var success = false;
                if (emailMessage != null)
                {
                    // add cc & bcc
                    AddTaskCcToEmailMessage(emailMessage);
                    AddTaskBccToEmailMessage(emailMessage);
                    Log.Debug(emailMessage.ToDebugString());

                    success = EmailDeliveryService.SendTaskReportEmail(emailMessage,
                        _r2UtilitiesSettings.DefaultFromAddress,
                        _r2UtilitiesSettings.DefaultFromAddressName);
                }


                step.CompletedSuccessfully = success;
                step.Results = "Process Web Activity Report completed successfully";

                return success;
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

        private WebActivityReport GetWebActivityReport()
        {
            TaskResult.Information = " Get Web Activity Report";
            var step = new TaskResultStep { Name = "GetWebActivityReport", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                var webActivityReport = _webActivityService.GetWebActivityReport(_startDate);

                step.CompletedSuccessfully = true;
                step.Results = "Get Web Activity Report completed successfully";

                return webActivityReport;
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

        private static string PopulateValue(string formatedString, string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "" : string.Format(formatedString, value);
        }

        private static string PopulateValue(string formatedString, int value)
        {
            return value == 0 ? "" : string.Format(formatedString, value);
        }


        private static string GetResultDetails(TopInstitution topInstitution)
        {
            return $"{topInstitution.AccountName}, [{topInstitution.AccountNumber}], ({topInstitution.InstitutionId})";
        }

        private static string GetResultDetails(TopResource topResource)
        {
            return $"{topResource.Isbn} - {topResource.Title}, ({topResource.ResourceId})";
        }

        private string GetResultDetails(TopIpAddress topIpRange)
        {
            if (string.IsNullOrWhiteSpace(topIpRange.AccountNumber))
            {
                var externalDescription = GetExternalIpInformation(topIpRange);
                if (!string.IsNullOrWhiteSpace(externalDescription))
                {
                    return externalDescription;
                }
            }

            return
                $"{topIpRange.OctetA}.{topIpRange.OctetB}.{topIpRange.OctetC}.{topIpRange.OctetD} --- {PopulateValue("{0}", topIpRange.AccountName)}{PopulateValue(", [{0}]", topIpRange.AccountNumber)}{PopulateValue(", ({0})", topIpRange.InstitutionId)}{PopulateValue(", [{0}]", topIpRange.CountryCode)}";
        }

        private string GetExternalIpInformation(TopIpAddress topIpRange)
        {
            try
            {
                var ipAddress = $"{topIpRange.OctetA}.{topIpRange.OctetB}.{topIpRange.OctetC}.{topIpRange.OctetD}";

                var request = WebRequest.Create($"https://ipapi.co/{ipAddress}/json/");

                //WebRequest request =WebRequest.Create($"http://api.ipstack.com/{ipAddress}?access_key=79a80dc5e3880d75eb5b3f81506d3ca9&output=json");
                //WebRequest request =
                //    WebRequest.Create(
                //        string.Format("http://api.geoips.com/ip/{0}/key/{1}/output/json",
                //            ipAddress, _r2UtilitiesSettings.GeoIPsApiKey));
                //GeoIPsApiKey
                request.Credentials = CredentialCache.DefaultCredentials;

                var response = (HttpWebResponse)request.GetResponse();

                var dataStream = response.GetResponseStream();

                string responseFromServer = null;
                if (dataStream != null)
                {
                    var reader = new StreamReader(dataStream);
                    responseFromServer = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                }

                response.Close();

                var jsonResponse = JObject.Parse(responseFromServer);

                var organization = jsonResponse["org"];
                var countryCode = jsonResponse["country"];

                Log.Info($"GetExternalIpInformation Response Information: {jsonResponse}");

                return
                    $"{topIpRange.OctetA}.{topIpRange.OctetB}.{topIpRange.OctetC}.{topIpRange.OctetD} --- {organization}[{countryCode}]";
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }

            return null;
        }

        private static string GetResultDetails(TopInstitutionResource topInstitutionsAndResources)
        {
            return
                $"{topInstitutionsAndResources.AccountName}, [{topInstitutionsAndResources.AccountNumber}], ({topInstitutionsAndResources.InstitutionId}) --- {topInstitutionsAndResources.Isbn} - {topInstitutionsAndResources.Title}, ({topInstitutionsAndResources.ResourceId})";
        }
    }
}