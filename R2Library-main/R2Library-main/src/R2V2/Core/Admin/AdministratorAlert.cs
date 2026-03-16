#region

using System;
using System.Collections.Generic;
using R2V2.Core.Authentication;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Admin
{
    [Serializable]
    public class AdministratorAlert : AuditableEntity, ISoftDeletable, IAdminAlert
    {
        public virtual bool DisplayOnce { get; set; }

        public virtual string Title { get; set; }

        public virtual string Text { get; set; }

        public virtual AlertLayout Layout { get; set; }

        public virtual IEnumerable<AlertImage> AlertImages { get; set; }

        public virtual DateTime? StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public virtual string AlertName { get; set; }

        public virtual Role Role { get; set; }

        public virtual int RoleId { get; set; }

        public virtual int? ResourceId { get; set; }
        public virtual bool AllowPurchase { get; set; }
        public virtual bool AllowPDA { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}