namespace R2V2.Infrastructure
{
    public interface IRequestInformation
    {
        string Id { get; }
        string ClientAddress { get; }
        string ReferringUrl { get; }
        string SessionId { get; }
        string Host { get; }
        string Url { get; }
        string Summary();
        string Details();
    }
}