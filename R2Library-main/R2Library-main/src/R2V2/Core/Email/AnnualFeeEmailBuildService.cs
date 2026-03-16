#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.Reports;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class AnnualFeeEmailBuildService : EmailBuildBaseService
    {
        public AnnualFeeEmailBuildService(
            ILog<EmailBuildBaseService> log,
            IEmailSettings emailSettings,
            IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
            SetTemplates(AnnualFeeBodyTemplate, AnnualFeeBodyTemplate, false);
        }

        public EmailMessage BuildAnnualFeeEmail(List<AnnualFeeReportDataItem> annualFeeItems, User user)
        {
            var messageHtml = GetAnnualFeeEmailHtml(annualFeeItems, user);

            return BuildEmailMessage(user, "R2 Library Annual Maintenance Fee Report", messageHtml);
        }

        private string GetAnnualFeeEmailHtml(List<AnnualFeeReportDataItem> annualFeeItems, User user)
        {
            //PopulateField("Practice Area: ", resource.PracticeAreasToString()))
            var items = new StringBuilder();
            foreach (var item in annualFeeItems.Select(annualFeeItem => ItemTemplate
                         .Replace("{AnnualFeeItem_AccountNumber}",
                             PopulateField("Account Number: ", annualFeeItem.AccountNumber))
                         .Replace("{AnnualFeeItem_InstitutionName}",
                             PopulateField("Institution Name: ", annualFeeItem.InstitutionName))
                         .Replace("{AnnualFeeItem_Consortia}", PopulateField("Consortia: ", annualFeeItem.Consortia))
                         .Replace("{AnnualFeeItem_ActiveDate}", PopulateField("Active Date: ",
                             $"{annualFeeItem.ActiveDate:MM/dd/yyyy}"))
                         .Replace("{AnnualFeeItem_ContactName}",
                             PopulateField("Contact Name: ", annualFeeItem.ContactName))
                         .Replace("{AnnualFeeItem_RenewalDate}", PopulateField("Current Renewal Date: ",
                             $"{annualFeeItem.RenewalDate:MM/dd/yyyy}"))
                         .Replace("{AnnualFeeItem_ContactEmail}",
                             PopulateField("Contact Email: ", annualFeeItem.ContactEmail))
                     ))
            {
                items.Append(item);
            }

            var bodyBuilder = BuildBodyHtml()
                .Replace("{AnnualFee_Date}", $"{DateTime.Now:MM/dd/yyyy}")
                .Replace("{AnnualFee_InstitutionCount}", annualFeeItems.Count.ToString())
                .Replace("{AnnualFee_Body}", items.ToString());

            var mainBuilder = BuildMainHtml("Annual Maintenance Fee Report", bodyBuilder, user);

            return mainBuilder;
        }
    }
}