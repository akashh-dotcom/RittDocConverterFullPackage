#region

using System.Collections.Generic;

#endregion

namespace R2V2.Web.Models.AlphaIndex
{
    public class Topics : BaseModel, IPageable
    {
        public AlphaQuery AlphaQuery { get; set; }
        public IEnumerable<Topic> TopicList { get; set; }

        public IEnumerable<PageLink> PageLinks { get; set; }
    }
}