#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Admin
{
    [Serializable]
    public class UserAlert : AuditableEntity, ISoftDeletable
    {
        public virtual int? UserId { get; set; }

        public virtual int? PublisherUserId { get; set; }

        public virtual int AlertId { get; set; }
        public virtual bool RecordStatus { get; set; }


        public virtual string ToDebugString()
        {
            return new StringBuilder()
                .Append("UserAlert = [")
                .AppendFormat("UserId: {0}||", UserId)
                .AppendFormat("PublisherUserId: {0}||", PublisherUserId)
                .AppendFormat("AlertId: {0}]", AlertId)
                .ToString();
        }
    }
}