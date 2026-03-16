#region

using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models
{
    public class MarcRecordExport : BaseModel
    {
        public string MarcRecordRequestString { get; set; }

        public string Url { get; set; }
    }
}