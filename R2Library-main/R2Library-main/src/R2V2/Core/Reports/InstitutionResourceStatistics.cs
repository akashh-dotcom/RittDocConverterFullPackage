namespace R2V2.Core.Reports
{
    public class InstitutionResourceStatistics
    {
        public virtual int ResourceId { get; set; }

        public virtual bool ExpertRecommended { get; set; }

        public virtual bool PdaAdded { get; set; }
        public virtual bool PdaAddedToCart { get; set; }
        public virtual bool PdaNewEdition { get; set; }


        public virtual bool Purchased { get; set; }
        public virtual bool ArchivedPurchased { get; set; }
        public virtual bool NewEditionPreviousPurchased { get; set; }
    }
}