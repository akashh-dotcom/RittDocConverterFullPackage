#region

using R2V2.Web.Models.Contact;

#endregion

namespace R2V2.Web.Models.Cms
{
    public class CmsHtml : BaseModel
    {
        public string Title { get; set; }
        public string Html { get; set; }

        public string FormHeader { get; set; }
        public string FormRecipients { get; set; }

        public string SecurityMessage { get; set; }

        public ContactUs ContactInfo { get; set; }
    }
}