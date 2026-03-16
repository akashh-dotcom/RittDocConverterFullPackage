#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.Core.AutomatedCart
{
    public class AutomatedCartReport
    {
        public IInstitution Institution { get; set; }
        public bool NewEdition { get; set; }
        public bool TriggeredPda { get; set; }
        public bool Reviewed { get; set; }
        public bool Turnaway { get; set; }
        public bool Requested { get; set; }
    }

    public class AutomatedCartPricedReport
    {
        public IInstitution Institution { get; set; }
        public int NewEditionCount { get; set; }
        public int TriggeredPdaCount { get; set; }
        public int ReviewedCount { get; set; }
        public int TurnawayCount { get; set; }
        public int RequestedCount { get; set; }
        public int ResourceCount { get; set; }
        public decimal ListPrice { get; set; }
        public decimal DiscountPrice { get; set; }
    }
}