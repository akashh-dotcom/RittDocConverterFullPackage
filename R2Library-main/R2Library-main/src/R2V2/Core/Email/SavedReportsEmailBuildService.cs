#region

using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Mime;
using R2V2.Core.Authentication;
using R2V2.Core.Export.FileTypes;
using R2V2.Core.Institution;
using R2V2.Core.Reports;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class SavedReportsEmailBuildService : EmailBuildBaseService
    {
        public SavedReportsEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
        }

        public void InitEmailTemplates()
        {
            SetTemplates(ResourceUsageBodyTemplate);
        }

        public EmailMessage BuildApplicationUsageReportEmail(ApplicationReportCounts applicationReportCounts,
            SavedReport savedReport, ReportRequest reportRequest, IInstitution institution, IUser user)
        {
            var messageHtml =
                GetApplicationUsageEmailHtml(applicationReportCounts, savedReport, reportRequest, institution, user);

            if (string.IsNullOrWhiteSpace(messageHtml))
            {
                return null;
            }

            return BuildEmailMessage(new[] { savedReport.Email }, "R2 Library Automated Report", messageHtml);
        }

        private string GetApplicationUsageEmailHtml(ApplicationReportCounts applicationReportCounts,
            SavedReport savedReport, ReportRequest reportRequest, IInstitution institution, IUser user)
        {
            SetTemplates(ApplicationUsageBodyTemplate);

            var bodyHtml = BuildBodyHtml()
                    .Replace("{Report_Description}", savedReport.Description)
                    .Replace("{UserSessions}", applicationReportCounts.UserSessionCount.ToString())
                    .Replace("{PageViews}", applicationReportCounts.PageViewCount.ToString())
                    .Replace("{ContentRetrievals}", applicationReportCounts.RestrictedContentRetrievalCount.ToString())
                    .Replace("{TocRetrievals}", applicationReportCounts.TocOnlyContentRetrievalCount.ToString())
                    .Replace("{ConcurrencyTurnaways}", applicationReportCounts.ConcurrencyTurnawayCount.ToString())
                    .Replace("{AccessTurnaways}", applicationReportCounts.AccessTurnawayCount.ToString())
                    .Replace("{Search_ActiveContent}", applicationReportCounts.SearchActiveCount.ToString())
                    .Replace("{Search_ArchivedContent}", applicationReportCounts.SearchArchiveCount.ToString())
                    .Replace("{Search_Image}", applicationReportCounts.SearchImageCount.ToString())
                    .Replace("{Search_Drug}", applicationReportCounts.SearchDrugCount.ToString())
                    .Replace("{PubMed}", applicationReportCounts.SearchPubMedCount.ToString())
                    .Replace("{Mesh}", applicationReportCounts.SearchMeshCount.ToString())
                ;
            if (user.IsRittenhouseAdmin())
            {
                bodyHtml = bodyHtml.Replace("{Pda_Section}",
                    GetTemplateFromFile("ApplicationUsage_Pda.html")
                        .Replace("{Pda_Total}", applicationReportCounts.PdaTotalCount.ToString())
                        .Replace("{Pda_Active}", applicationReportCounts.PdaActiveCount.ToString())
                        .Replace("{Pda_Cart}", applicationReportCounts.PdaCartCount.ToString())
                        .Replace("{Pda_Purchased}", applicationReportCounts.PdaPurchasedCount.ToString())
                );
            }
            else
            {
                bodyHtml = bodyHtml.Replace("{Pda_Section}", "");
            }

            var title =
                $"Application Usage Report for {reportRequest.DateRangeStart.ToShortDateString()} to {reportRequest.DateRangeEnd.ToShortDateString()}";

            var mainHtml = BuildMainHtml(title, bodyHtml, savedReport.Email, institution);

            return mainHtml;
        }

        public EmailMessage BuildResourceUsageReportEmail(List<ResourceReportItem> items, SavedReport savedReport,
            ReportRequest reportRequest, IInstitution institution)
        {
            var excelExport = new ResourceUsageExcelExport(items, false, institution.ProxyPrefix, institution.UrlSuffix,
                GetResourceLink(null, institution.AccountNumber), false);

            var contentType = new ContentType
            {
                Name =
                    $"R2_ResourceUsage_{reportRequest.DateRangeStart.ToShortDateString()} to {reportRequest.DateRangeEnd.ToShortDateString()}.xlsx"
            };

            var attachment = new Attachment(excelExport.Export(), contentType)
                { ContentType = { MediaType = excelExport.MimeType } };

            var messageHtml = GetResourceUsageEmailHtml(savedReport, reportRequest, institution);

            return BuildEmailMessage(savedReport.Email, null, "R2 Library Automated Report", messageHtml, attachment);
        }

        private string GetResourceUsageEmailHtml(SavedReport savedReport, ReportRequest reportRequest,
            IInstitution institution)
        {
            var bodyHtml = BuildBodyHtml()
                .Replace("{Report_Description}", savedReport.Description);

            var title =
                $"Resource Usage Report for {reportRequest.DateRangeStart.ToShortDateString()} to {reportRequest.DateRangeEnd.ToShortDateString()}";

            var mainHtml = BuildMainHtml(title, bodyHtml, savedReport.Email, institution);

            return mainHtml;
        }
    }
}