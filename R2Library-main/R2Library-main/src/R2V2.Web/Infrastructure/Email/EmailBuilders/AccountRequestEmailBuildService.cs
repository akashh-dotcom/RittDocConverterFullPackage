#region

using System;
using System.Text;
using System.Web.Mvc;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Web.Models.Trial;

#endregion

namespace R2V2.Web.Infrastructure.Email.EmailBuilders
{
    public class AccountRequestEmailBuildService : EmailBuildBaseService
    {
        private readonly ILog<EmailBuildBaseService> _log;

        public AccountRequestEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
            _log = log;
            SetTemplates(MainHeaderFooterTemplate, AccountRequestBodyTemplate, AccountRequestItemTemplate);
        }

        public string GetMessageBody(RequestAccountModel model, UrlHelper urlHelper)
        {
            var html = new StringBuilder();
            var body = new StringBuilder();

            body.Append(BodyTemplate
                .Replace("{Company_Items}", GetCompanyInformationItems(model.CompanyInformation))
                .Replace("{Billing_Items}", GetCompanyAddressItems(model.CompanyAddresses.BillingInformation))
                .Replace("{Shipping_Items}", GetCompanyAddressItems(model.CompanyAddresses.ShippingInformation))
                .Replace("{Payable_Items}",
                    GetAccountsPayableItems(model.PayableInformation.AccountsPayableInformation))
                .Replace("{Purchasing_Items}", GetAccountsPayableItems(model.PayableInformation.PurchasingInformation))
                .Replace("{Banking_Items}", GetAccountsPayableItems(model.PayableInformation.BankingInformation))
                .Replace("{Trade1_Items}", GetReferenceItems(model.TradeReferences.Reference1))
                .Replace("{Trade2_Items}", GetReferenceItems(model.TradeReferences.Reference2))
                .Replace("{Trade3_Items}", GetReferenceItems(model.TradeReferences.Reference3))
            );

            html.Append(MainTemplate
                .Replace("{Title}", "Rittenhouse Account Request")
                .Replace("{Body}", body.ToString())
                .Replace("{Year}", DateTime.Now.Year.ToString())
                .Replace("{WebsiteUrl}", urlHelper.Action("Index", "Home", new { Area = "" }, "https"))
                .Replace("{User_Email}", model.CompanyInformation.Email)
                .Replace("{Institution_Name}", model.CompanyInformation.ContactAddress.Name)
                .Replace("(#{Institution_Number}) -", "")
            );
            return html.ToString();
        }

        private string GetCompanyInformationItems(CompanyInformation companyInformation)
        {
            var items = new StringBuilder();

            items.Append(ItemTemplate.Replace("{Item_Labal}", "Name of Business:")
                .Replace("{Item_Value}", companyInformation.ContactAddress.Name));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Business Type:")
                .Replace("{Item_Value}", companyInformation.Type));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Name of Officer:")
                .Replace("{Item_Value}", companyInformation.OfficerName));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Officer Title:")
                .Replace("{Item_Value}", companyInformation.OfficerTitle));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Address 1:")
                .Replace("{Item_Value}", companyInformation.ContactAddress.Address1));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Address 2")
                .Replace("{Item_Value}", companyInformation.ContactAddress.Address2));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "City:")
                .Replace("{Item_Value}", companyInformation.ContactAddress.City));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "State:")
                .Replace("{Item_Value}", companyInformation.ContactAddress.State));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Zip Code:")
                .Replace("{Item_Value}", companyInformation.ContactAddress.Zip));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Country:")
                .Replace("{Item_Value}", companyInformation.ContactAddress.Country));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Email:")
                .Replace("{Item_Value}", companyInformation.Email));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Phone:")
                .Replace("{Item_Value}", companyInformation.ContactAddress.Phone));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Fax:")
                .Replace("{Item_Value}", companyInformation.ContactAddress.Fax));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Website:")
                .Replace("{Item_Value}", companyInformation.Website));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "San #:")
                .Replace("{Item_Value}", companyInformation.SanNumber));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "EIN:").Replace("{Item_Value}", companyInformation.Ein));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Duns #:")
                .Replace("{Item_Value}", companyInformation.DunsNumber));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Consortium:")
                .Replace("{Item_Value}", companyInformation.Consortium));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Store #/Customer ID Code:")
                .Replace("{Item_Value}", companyInformation.CustomerId));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Affiliated Institution:")
                .Replace("{Item_Value}", companyInformation.AffiliatedInstitution));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Account Expiration:")
                .Replace("{Item_Value}", companyInformation.AccountExpiration));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Max Billing Limit:")
                .Replace("{Item_Value}", companyInformation.MaxBillingLimit));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Sales Tax Exemption #:")
                .Replace("{Item_Value}", companyInformation.SalesTaxExemptionNumber));

            return items.ToString();
        }

        private string GetCompanyAddressItems(AddressInformation addressInformation)
        {
            var items = new StringBuilder();
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Name of Business / Contact:")
                .Replace("{Item_Value}", addressInformation.ContactAddress.Name));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Name of Officer:")
                .Replace("{Item_Value}", addressInformation.OfficerName));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Address 1:")
                .Replace("{Item_Value}", addressInformation.ContactAddress.Address1));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Address 2:")
                .Replace("{Item_Value}", addressInformation.ContactAddress.Address2));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "City:")
                .Replace("{Item_Value}", addressInformation.ContactAddress.City));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "State:")
                .Replace("{Item_Value}", addressInformation.ContactAddress.State));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Zip Code:")
                .Replace("{Item_Value}", addressInformation.ContactAddress.Zip));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Country:")
                .Replace("{Item_Value}", addressInformation.ContactAddress.Country));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Phone:")
                .Replace("{Item_Value}", addressInformation.ContactAddress.Phone));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Fax:")
                .Replace("{Item_Value}", addressInformation.ContactAddress.Fax));

            return items.ToString();
        }

        private string GetAccountsPayableItems(AccountsPayableInformation accountsPayableInformation)
        {
            var items = new StringBuilder();
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Contact:")
                .Replace("{Item_Value}", accountsPayableInformation.ContactName));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Email:")
                .Replace("{Item_Value}", accountsPayableInformation.Email));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Phone:")
                .Replace("{Item_Value}", accountsPayableInformation.Phone));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Fax:")
                .Replace("{Item_Value}", accountsPayableInformation.Fax));
            return items.ToString();
        }

        private string GetAccountsPayableItems(PurchasingInformation purchasingInformation)
        {
            var items = new StringBuilder();
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Contact:")
                .Replace("{Item_Value}", purchasingInformation.ContactName));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Email:")
                .Replace("{Item_Value}", purchasingInformation.Email));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Phone:")
                .Replace("{Item_Value}", purchasingInformation.Phone));
            items.Append(
                ItemTemplate.Replace("{Item_Labal}", "Fax:").Replace("{Item_Value}", purchasingInformation.Fax));
            return items.ToString();
        }

        private string GetAccountsPayableItems(BankingInformation bankingInformation)
        {
            var items = new StringBuilder();
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Name:")
                .Replace("{Item_Value}", bankingInformation.Name));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Account #:")
                .Replace("{Item_Value}", bankingInformation.AccountNumber));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Contact:")
                .Replace("{Item_Value}", bankingInformation.ContactName));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Phone:")
                .Replace("{Item_Value}", bankingInformation.Phone));
            return items.ToString();
        }

        private string GetReferenceItems(References reference)
        {
            var items = new StringBuilder();
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Name:").Replace("{Item_Value}", reference.Name));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Address 1:")
                .Replace("{Item_Value}", reference.Address1));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Address 2:")
                .Replace("{Item_Value}", reference.Address2));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "City:").Replace("{Item_Value}", reference.City));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "State:").Replace("{Item_Value}", reference.State));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Zip Code:").Replace("{Item_Value}", reference.Zip));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Phone:").Replace("{Item_Value}", reference.Phone));
            items.Append(ItemTemplate.Replace("{Item_Labal}", "Account #:")
                .Replace("{Item_Value}", reference.AccountNumber));
            return items.ToString();
        }
    }
}