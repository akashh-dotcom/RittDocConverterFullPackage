#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

#endregion

namespace R2V2.Web.Models.Trial
{
    public class RequestAccountModel : BaseModel
    {
        public RequestAccountModel()
        {
            CompanyInformation = new CompanyInformation();
            CompanyAddresses = new CompanyAddresses();
            PayableInformation = new PayableInformation();
            TradeReferences = new TradeReferences();

            Sections = new List<PageLink>
            {
                new PageLink
                {
                    Selected = string.IsNullOrWhiteSpace(CurrentSection) || CurrentSection == "1",
                    Href = "#1",
                    Text = "Company Information"
                },
                new PageLink
                {
                    Selected = CurrentSection == "2",
                    Href = "#2",
                    Text = "Billing/Shipping"
                },
                new PageLink
                {
                    Selected = CurrentSection == "3",
                    Href = "#3",
                    Text = "Payables"
                },
                new PageLink
                {
                    Selected = CurrentSection == "4",
                    Href = "#4",
                    Text = "References"
                },
                new PageLink
                {
                    Selected = CurrentSection == "5",
                    Href = "#5",
                    Text = "Review"
                }
            };
        }

        public IEnumerable<PageLink> Sections { get; set; }
        public string CurrentSection { get; set; }

        public CompanyInformation CompanyInformation { get; set; }
        public CompanyAddresses CompanyAddresses { get; set; }
        public PayableInformation PayableInformation { get; set; }
        public TradeReferences TradeReferences { get; set; }
    }

    public class TradeReferences
    {
        public References Reference1 { get; set; }
        public References Reference2 { get; set; }
        public References Reference3 { get; set; }
    }

    public class CompanyInformation
    {
        private SelectList _typeList;
        public ContactAddress ContactAddress { get; set; }
        public string Type { get; set; }

        public SelectList TypeList => new SelectList(new List<SelectListItem>
        {
            new SelectListItem { Text = @"", Value = "" },
            new SelectListItem { Text = @"Sole Proprietorship", Value = "Sole Proprietorship" },
            new SelectListItem { Text = @"Partnership", Value = "Partnership" },
            new SelectListItem { Text = @"Corporation", Value = "Corporation" }
        }, "Value", "Text", Type);

        public string OfficerName { get; set; }
        public string OfficerTitle { get; set; }
        [Required] public string Email { get; set; }
        public string Website { get; set; }
        public string SanNumber { get; set; }
        public string Ein { get; set; }
        public string DunsNumber { get; set; }
        public string Consortium { get; set; }
        public string CustomerId { get; set; }
        public string AffiliatedInstitution { get; set; }
        public string AccountExpiration { get; set; }
        public string MaxBillingLimit { get; set; }
        public string SalesTaxExemptionNumber { get; set; }
    }

    public class CompanyAddresses
    {
        public AddressInformation BillingInformation { get; set; }
        public AddressInformation ShippingInformation { get; set; }
    }

    public class PayableInformation
    {
        public AccountsPayableInformation AccountsPayableInformation { get; set; }
        public PurchasingInformation PurchasingInformation { get; set; }
        public BankingInformation BankingInformation { get; set; }
    }

    public class AddressInformation
    {
        public ContactAddress ContactAddress { get; set; }
        public string OfficerName { get; set; }
    }

    public class AccountsPayableInformation
    {
        [Required] public string ContactName { get; set; }
        [Required] public string Email { get; set; }
        [Required] public string Phone { get; set; }
        public string Fax { get; set; }
    }

    public class PurchasingInformation
    {
        [Required] public string ContactName { get; set; }
        [Required] public string Email { get; set; }
        [Required] public string Phone { get; set; }
        public string Fax { get; set; }
    }

    public class BankingInformation
    {
        [Required] public string Name { get; set; }
        [Required] public string AccountNumber { get; set; }
        [Required] public string ContactName { get; set; }
        [Required] public string Phone { get; set; }
    }


    public class References : FormAddress
    {
        [Required] public string AccountNumber { get; set; }
    }

    public class ContactAddress : FormAddress
    {
        [Required] public string Fax { get; set; }
    }

    public class FormAddress
    {
        [Required] public string Address1 { get; set; }
        public string Address2 { get; set; }
        [Required] public string City { get; set; }
        [Required] public string State { get; set; }
        [Required] public string Zip { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string Phone { get; set; }
        public string Country { get; set; }
    }
}