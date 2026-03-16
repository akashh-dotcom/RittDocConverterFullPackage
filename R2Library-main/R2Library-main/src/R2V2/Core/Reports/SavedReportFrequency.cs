namespace R2V2.Core.Reports
{
    public enum ReportFrequency
    {
        None = 0,
        Weekly = 7,
        BiWeekly = 14,
        Monthly = 30
    }

    public static class SavedReportFrequencyExtensions
    {
        public static string ToDescription(this ReportFrequency frequency)
        {
            switch (frequency)
            {
                case ReportFrequency.Weekly:
                    return "Weekly";
                case ReportFrequency.BiWeekly:
                    return "Bi-Weekly";
                case ReportFrequency.Monthly:
                    return "Monthly";
                case ReportFrequency.None:
                default:
                    return "Do Not Schedule Email";
            }
        }
    }
}