#region

using System;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;

#endregion

namespace R2V2.Core.AutomatedCart
{
    [Serializable]
    public class AutomatedCartInstitutionSummary
    {
        public AutomatedCartInstitutionSummary(IInstitution institution)
        {
            AccountNumber = institution.AccountNumber;
            InstitutionName = institution.Name;
            InstitutionId = institution.Id;
            Address = institution.Address;
            if (institution.Type != null)
            {
                InstitutionType = institution.Type.Name;
            }

            if (institution.Territory != null)
            {
                Territory = institution.Territory.Code;
            }

            NewEditionCount = 0;
            PdaCount = 0;
            ReviewedCount = 0;
            TurnawayCount = 0;
            RequestedCount = 0;
            TitleCount = 0;
            ListPrice = 0;
            DiscountPrice = 0;
            EmailCount = 0;
        }

        public string AccountNumber { get; }
        public string InstitutionName { get; }
        public int InstitutionId { get; }
        public Address Address { get; }
        public string InstitutionType { get; }
        public string Territory { get; }
        public int NewEditionCount { get; set; }
        public int PdaCount { get; set; }
        public int ReviewedCount { get; set; }
        public int TurnawayCount { get; set; }
        public int RequestedCount { get; set; }
        public int TitleCount { get; set; }
        public int EmailCount { get; set; }
        public bool AllEmailsSuccessful { get; set; }
        public decimal ListPrice { get; set; }
        public decimal DiscountPrice { get; set; }

        public int CartId { get; set; }
        public bool CartExists { get; set; }
    }
}