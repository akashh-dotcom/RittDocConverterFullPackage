#region

using R2V2.Core.Reports;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class SavedReportIpFilterMap : BaseMap<SavedReportIpFilter>
    {
        public SavedReportIpFilterMap()
        {
            //select iIPFilterId, vchIpStartRange, vchIpEndRange, tiRecordStatus, iReportId from tSavedReportsIpFilter
            Table("tSavedReportsIpFilter");

            Id(x => x.Id).Column("iIPFilterId").GeneratedBy.Identity();

            //Map(x => x.ReportId, "iReportId");
            References(x => x.Report).Column("iReportId");
            Map(x => x.IpStartRange, "vchIpStartRange");
            Map(x => x.IpEndRange, "vchIpEndRange");
        }
    }
}