namespace R2V2.Core.Resource
{
    public enum ResourceAccess
    {
        Allowed,
        Locked, // Concurrency turn away
        Denied // No Access turn away
    }
}