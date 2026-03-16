#region

using System;

#endregion

namespace R2V2.Core.Search
{
    public class SearchTypeahead
    {
        public virtual int Id { get; set; }
        public virtual string SearchTerm { get; set; }
        public virtual int Rank { get; set; }
        public virtual string CreatorId { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual string UpdaterId { get; set; }
        public virtual DateTime UpdateDate { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}