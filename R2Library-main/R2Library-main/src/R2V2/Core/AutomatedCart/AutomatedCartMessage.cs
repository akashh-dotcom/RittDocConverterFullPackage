#region

using System;
using Newtonsoft.Json;
using R2V2.Core.Reports;
using R2V2.Infrastructure.MessageQueue;

#endregion

namespace R2V2.Core.AutomatedCart
{
    [Serializable]
    public class AutomatedCartMessage : IR2V2Message
    {
        public AutomatedCartMessage()
        {
            MessageId = Guid.NewGuid();
        }

        public AutomatedCartMessage(DbAutomatedCart automatedCart, int[] selectedInstitutionIds, int[] territoryIds,
            int[] typeIds)
        {
            AutomatedCartId = automatedCart.Id;
            MessageId = Guid.NewGuid();
            Period = automatedCart.Period;
            StartDate = automatedCart.StartDate;
            EndDate = automatedCart.EndDate;
            NewEdition = automatedCart.NewEdition;
            TriggeredPda = automatedCart.TriggeredPda;
            Reviewed = automatedCart.Reviewed;
            Turnaway = automatedCart.Turnaway;
            Discount = automatedCart.Discount;
            AccountNumbers = automatedCart.AccountNumbers;
            CartName = automatedCart.CartName;
            EmailText = automatedCart.EmailText;
            SelectedInstitutionIds = selectedInstitutionIds;
            TerritoryIds = territoryIds;
            TypeIds = typeIds;
        }

        public int AutomatedCartId { get; set; }
        public ReportPeriod Period { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool NewEdition { get; set; }
        public bool TriggeredPda { get; set; }
        public bool Reviewed { get; set; }
        public bool Turnaway { get; set; }
        public decimal Discount { get; set; }
        public string AccountNumbers { get; set; }
        public string CartName { get; set; }
        public string EmailText { get; set; }
        public int[] SelectedInstitutionIds { get; set; }
        public int[] TerritoryIds { get; set; }
        public int[] TypeIds { get; set; }
        public int FailedSaveAttempts { get; set; }

        public Guid MessageId { get; set; }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}