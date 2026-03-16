namespace R2Utilities.Tasks
{
    public enum TaskGroup
    {
        None,
        ContentLoading,
        DiagnosticsMaintenance,
        CustomerEmails,
        RittenhouseOnlyEmails,
        InternalSystemEmails,
        Feeds,
        Deprecated
    }

    public class TaskGroups
    {
        public static string[] Names =
        {
            "None",
            "Content Loading",
            "Diagnostics & Maintenance",
            "CustomerEmails",
            "RittenhouseOnlyEmails",
            "InternalSystemEmails",
            "Feeds",
            "Deprecated"
        };
    }
}