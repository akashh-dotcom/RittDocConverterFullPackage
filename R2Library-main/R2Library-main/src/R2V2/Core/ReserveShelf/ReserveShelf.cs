#region

using System;
using System.Collections.Generic;
using R2V2.Core.Institution;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.ReserveShelf
{
    [Serializable]
    public class ReserveShelf : AuditableEntity, ISoftDeletable
    {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string DefaultSortBy { get; set; }
        public virtual bool? IsAscending { get; set; }

        public virtual IInstitution Institution { get; set; }

        public virtual IEnumerable<ReserveShelfResource> ReserveShelfResources { get; set; }
        public virtual IEnumerable<ReserveShelfUrl> ReserveShelfUrls { get; set; }

        public virtual int LibraryLocation { get; set; }

        public virtual bool RecordStatus { get; set; }
    }
}