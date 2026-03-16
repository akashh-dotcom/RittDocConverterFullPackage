#region

using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Reports
{
    public class SavedReportIpFilter : ISoftDeletable
    {
        public virtual int Id { get; set; }

        //public virtual int ReportId { get; set; }
        public virtual SavedReport Report { get; set; }
        public virtual string IpStartRange { get; set; }
        public virtual string IpEndRange { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}