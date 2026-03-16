#region

using System;
using R2V2.Core.Institution;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Institution
{
    public class InstitutionForAudit
    {
        public InstitutionForAudit(InstitutionEditViewModel institutionEdit)
        {
            InstitutionId = institutionEdit.InstitutionId;
            AccountStatus = institutionEdit.AccountStatus;
            TrialEndDate = institutionEdit.TrialEndDate;
            Name = institutionEdit.InstitutionName;
            if (institutionEdit.InstitutionTerritory != null)
            {
                TerritoryId = institutionEdit.InstitutionTerritory.TerritoryId;
            }

            DisplayNonPurchasedTitles = institutionEdit.DisplayAllProducts;
            HomePageId = institutionEdit.HomePageId;
            AccessType = institutionEdit.AccessType;
            HouseAccount = institutionEdit.HouseAccount;
            InstitutionDiscount = institutionEdit.Discount;
            TrustedSecurityKey = institutionEdit.TrustedKey;
            LogUrl = institutionEdit.LogUrl;
        }

        public int InstitutionId { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public string Name { get; set; }
        public int TerritoryId { get; set; }
        public bool DisplayNonPurchasedTitles { get; set; }
        public int HomePageId { get; set; }
        public AccessType AccessType { get; set; }
        public bool HouseAccount { get; set; }
        public decimal InstitutionDiscount { get; set; }
        public string TrustedSecurityKey { get; set; }
        public string LogUrl { get; set; }
    }
}