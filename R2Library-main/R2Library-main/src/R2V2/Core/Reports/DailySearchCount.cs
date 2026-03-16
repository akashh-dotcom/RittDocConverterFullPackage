namespace R2V2.Core.Reports
{
    public class DailySearchCount : DailyCountBase
    {
        public virtual int SearchTypeId { get; set; }
        public virtual bool IsArchived { get; set; }
        public virtual bool IsExternal { get; set; }
    }
}