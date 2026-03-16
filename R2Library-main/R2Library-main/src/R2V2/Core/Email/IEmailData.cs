namespace R2V2.Core.Email
{
    public interface IEmailData
    {
        string From { get; set; }
        string To { get; set; }
        string Cc { get; set; }
        string Bcc { get; set; }
        string Subject { get; set; }
        string Comments { get; set; }
    }
}