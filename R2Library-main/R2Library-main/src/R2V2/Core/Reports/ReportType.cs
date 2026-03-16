namespace R2V2.Core.Reports
{
    public enum ReportType
    {
        ApplicationUsageReport = 1,
        ResourceUsageReport = 2,
        AnnualFeeReport = 3,
        PdaCountsReport = 4,
        CounterSectionRequests = 5,
        CounterDeniedRequests = 6,
        CounterSearchRequests = 7,
        CounterPlatformRequests = 8,
        PublisherUser = 9,
        CounterBookRequests = 10,
        PublisherUsageReport = 11,

        SalesReport = 12
        //CounterBookAccessDeniedRequests = 11
    }

    public static class ReportTypeExtensions
    {
        public static string ToDescription(this ReportType type)
        {
            switch (type)
            {
                case ReportType.ApplicationUsageReport:
                    return "Application Usage";
                case ReportType.ResourceUsageReport:
                    return "Resource Usage";
                case ReportType.AnnualFeeReport:
                    return "Annual Maintenace Fee";
                case ReportType.PdaCountsReport:
                    return "PDA Usage";
                default:
                    return "Not Specified";
            }
            //return "";
        }
    }
}