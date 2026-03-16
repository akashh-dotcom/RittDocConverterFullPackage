#region

using System;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.MyR2
{
    [Serializable]
    public class UserSavedSearchResult : AuditableEntity, ISoftDeletable
    {
        public virtual string Title { get; set; }
        public virtual int ResultsCount { get; set; }

        public virtual string SearchResultSet { get; set; }
        public virtual string SearchQuery { get; set; }

        public virtual UserSavedSearchResultFolder Folder { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}