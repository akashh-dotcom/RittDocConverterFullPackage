#region

using System;

#endregion

namespace R2V2.Core.Resource
{
    public class NewResourceQue
    {
        public virtual int Id { get; set; }

        public virtual bool Processed { get; set; }
        public virtual bool RecordStatus { get; set; }
        public virtual int ResourceId { get; set; }

        public virtual DateTime? NewResourceEmailDate { get; set; }
        public virtual DateTime? NewEditionEmailDate { get; set; }
        public virtual DateTime? PurchasedEmailDate { get; set; }


        public virtual string CreatedBy { get; set; }
        public virtual DateTime CreationDate { get; set; }

        public virtual string UpdatedBy { get; set; }
        public virtual DateTime? LastUpdated { get; set; }
    }
}