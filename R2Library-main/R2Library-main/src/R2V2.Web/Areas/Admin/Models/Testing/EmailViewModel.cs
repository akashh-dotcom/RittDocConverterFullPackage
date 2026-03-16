namespace R2V2.Web.Areas.Admin.Models.Testing
{
    public class EmailViewModel : AdminBaseModel
    {
        public string Subject { get; set; }
        public string ToAddress { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string ReplyToAddress { get; set; }
        public string ReplyName { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; }
        public string StatusMessage { get; set; }
        public bool SentSuccessfully { get; set; }
    }
}