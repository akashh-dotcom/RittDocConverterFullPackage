#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.AutomatedCart;
using R2V2.Core.Institution;
using R2V2.Core.Reports;
using R2V2.Core.Territory;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class AutomatedCartSummaryEmailBuildService : EmailBuildBaseService
    {
        private readonly IQueryable<InstitutionType> _institutionTypes;
        private readonly ITerritoryService _territoryService;

        public AutomatedCartSummaryEmailBuildService(ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
            , IQueryable<InstitutionType> institutionTypes
            , ITerritoryService territoryService
        ) : base(log, emailSettings, contentSettings)
        {
            _institutionTypes = institutionTypes;
            _territoryService = territoryService;
            SetTemplates(MainHeaderFooterTemplate, AutomatedCartSummaryBodyTemplate, AutomatedCartSummaryItemTemplate,
                false,
                AutomatedCartSummaryInstitutionTemplate);
        }

        public EmailMessage BuildAutomatedCartSummaryEmail(DbAutomatedCart automatedCart,
            List<AutomatedCartInstitutionSummary> automatedCartInstitutionSummaries, string[] emailAddresses)
        {
            var messageBody = GetAutomatedCartSummaryHtml(automatedCart, automatedCartInstitutionSummaries);
            return BuildEmailMessage(emailAddresses, "R2 Library Automated Carts Summary", messageBody);
        }

        private string GetAutomatedCartSummaryHtml(DbAutomatedCart automatedCart,
            List<AutomatedCartInstitutionSummary> automatedCartInstitutionSummaries)
        {
            var bodyBuilder = BuildBodyHtml()
                    .Replace("{Query_Items}", GetQueryDetailsHtml(automatedCart, automatedCartInstitutionSummaries))
                    .Replace("{Institution_Items}", GetInstitutionDetails(automatedCartInstitutionSummaries))
                ;

            var mainBuilder = BuildMainHtml("Automated Shopping Carts Summary", bodyBuilder, null);

            return mainBuilder;
        }

        private string GetQueryDetailsHtml(DbAutomatedCart automatedCart,
            List<AutomatedCartInstitutionSummary> automatedCartInstitutionSummaries)
        {
            var institutionTypes = GetInstitutioinTypes(automatedCart.InstitutionTypeIds);
            var territories = GetTerritories(automatedCart.TerritoryIds);

            var sb = new StringBuilder();
            sb.Append(ItemTemplate.Replace("{Item_Labal}", "Period:").Replace("{Item_Value}",
                $"{GetPeriodDisplay(automatedCart.Period)} {automatedCart.StartDate:MM/dd/yyyy} to {automatedCart.EndDate:MM/dd/yyyy}"));

            sb.Append(ItemTemplate.Replace("{Item_Labal}", "Institution Type:").Replace("{Item_Value}",
                institutionTypes.Any() ? string.Join(", ", institutionTypes.Select(x => x.Name)) : ""));

            sb.Append(ItemTemplate.Replace("{Item_Labal}", "Territory:").Replace("{Item_Value}",
                territories.Any() ? string.Join(", ", territories.Select(x => x.Name)) : ""));


            sb.Append(ItemTemplate.Replace("{Item_Labal}", "Included Titles:")
                .Replace("{Item_Value}", GetIncludedTitles(automatedCart)));
            sb.Append(ItemTemplate.Replace("{Item_Labal}", "Included Account Numbers:")
                .Replace("{Item_Value}", automatedCart.AccountNumbers));
            sb.Append(ItemTemplate.Replace("{Item_Labal}", "Override Institution Discount:").Replace("{Item_Value}",
                automatedCart.Discount > 0 ? $"{automatedCart.Discount}%" : ""));
            sb.Append(
                ItemTemplate.Replace("{Item_Labal}", "Cart Name:").Replace("{Item_Value}", automatedCart.CartName));
            sb.Append(ItemTemplate.Replace("{Item_Labal}", "Email Subject:")
                .Replace("{Item_Value}", automatedCart.EmailSubject));
            sb.Append(ItemTemplate.Replace("{Item_Labal}", "Email Title:")
                .Replace("{Item_Value}", automatedCart.EmailTitle));
            sb.Append(ItemTemplate.Replace("{Item_Labal}", "Email Text:")
                .Replace("{Item_Value}", automatedCart.EmailText));
            sb.Append(ItemTemplate.Replace("{Item_Labal}", "Number of Institutions:")
                .Replace("{Item_Value}", automatedCartInstitutionSummaries.Count.ToString()));
            sb.Append(ItemTemplate.Replace("{Item_Labal}", "Number of Emails Sent:").Replace("{Item_Value}",
                automatedCartInstitutionSummaries.Sum(x => x.EmailCount).ToString()));
            return sb.ToString();
        }

        private List<InstitutionType> GetInstitutioinTypes(string institutionTypeIdString)
        {
            var institutionTypesToReturn = new List<InstitutionType>();
            var institutionTypes = _institutionTypes.ToList();
            var institutionTypeIdStringArray = !string.IsNullOrWhiteSpace(institutionTypeIdString)
                ? institutionTypeIdString.Split(',')
                : null;

            if (institutionTypeIdStringArray != null)
            {
                foreach (var s in institutionTypeIdStringArray)
                {
                    int.TryParse(s, out var i);
                    var type = institutionTypes.FirstOrDefault(x => x.Id == i);
                    if (type != null)
                    {
                        institutionTypesToReturn.Add(type);
                    }
                }
            }

            return institutionTypesToReturn;
        }

        private List<ITerritory> GetTerritories(string territoryIdString)
        {
            var territoriesToReturn = new List<ITerritory>();
            var territories = _territoryService.GetAllTerritories();

            var territoryIdStringArray = !string.IsNullOrWhiteSpace(territoryIdString)
                ? territoryIdString.Split(',')
                : null;

            if (territoryIdStringArray != null)
            {
                foreach (var s in territoryIdStringArray)
                {
                    int.TryParse(s, out var i);
                    var type = territories.FirstOrDefault(x => x.Id == i);
                    if (type != null)
                    {
                        territoriesToReturn.Add(type);
                    }
                }
            }

            return territoriesToReturn;
        }

        private string GetIncludedTitles(DbAutomatedCart automatedCart)
        {
            var sb = new StringBuilder();
            //New Edition Triggered PDA Expert Reviewed Turnaway
            if (automatedCart.NewEdition)
            {
                sb.Append("New Edition    ");
            }

            if (automatedCart.TriggeredPda)
            {
                sb.Append("    Triggered PDA    ");
            }

            if (automatedCart.Reviewed)
            {
                sb.Append("    Expert Reviewed    ");
            }

            if (automatedCart.Turnaway)
            {
                sb.Append("    Turnaway    ");
            }

            if (automatedCart.Requested)
            {
                sb.Append("    Requested");
            }

            return sb.ToString();
        }

        private string GetPeriodDisplay(ReportPeriod period)
        {
            switch (period)
            {
                case ReportPeriod.LastTwelveMonths:
                    return "Last 12 Months";
                case ReportPeriod.LastSixMonths:
                    return "Last 6 Months";
                case ReportPeriod.Last30Days:
                    return "Last 30 Days";
                case ReportPeriod.PreviousMonth:
                    return "Previous Month";
                case ReportPeriod.CurrentMonth:
                    return "Current Month";
                case ReportPeriod.UserSpecified:
                    return "Specified Date Range";
                case ReportPeriod.CurrentYear:
                    return $"{DateTime.Now.Year} Entire Year";
                case ReportPeriod.LastYear:
                    return $"{DateTime.Now.Year - 1} Entire Year";
                case ReportPeriod.CurrentQuarter:
                    return "Current Quarter";
                case ReportPeriod.PreviousQuarter:
                    return "Previous Quarter";
            }

            return null;
        }

        private string GetInstitutionDetails(List<AutomatedCartInstitutionSummary> automatedCartInstitutionSummaries)
        {
            var sb = new StringBuilder();
            automatedCartInstitutionSummaries.Reverse();

            foreach (var item in automatedCartInstitutionSummaries)
            {
                sb.Append(SubItemTemplate
                    .Replace("{Item_AccountNumber}", item.AccountNumber)
                    .Replace("{Item_InstitutionName}", item.InstitutionName)
                    .Replace("{Item_InstitutionType}", item.InstitutionType)
                    .Replace("{Item_Territory}", item.Territory)
                    .Replace("{Item_NewCount}", item.NewEditionCount.ToString())
                    .Replace("{Item_PdaCount}", item.PdaCount.ToString())
                    .Replace("{Item_ReviewedCount}", item.ReviewedCount.ToString())
                    .Replace("{Item_TurnawayCount}", item.TurnawayCount.ToString())
                    .Replace("{Item_RequestedCount}", item.RequestedCount.ToString())
                    .Replace("{Item_Titles}", item.TitleCount.ToString())
                    .Replace("{Item_ListPrice}", item.ListPrice.ToString("C"))
                    .Replace("{Item_DiscountPrice}", item.DiscountPrice.ToString("C"))
                    .Replace("{Item_EmailCount}", item.EmailCount.ToString())
                );
            }

            return sb.ToString();
        }
    }
}