namespace R2V2.Core.Reports
{
    public class DailyContentViewCount : DailyCountBase
    {
        public virtual int ResourceId { get; set; }
        public virtual string ChapterSectionId { get; set; }
        public virtual int ActionTypeId { get; set; }
    }
}