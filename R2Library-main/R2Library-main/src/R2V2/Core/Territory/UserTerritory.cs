#region

using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Territory
{
    public class UserTerritory : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual User User { get; set; }

        public virtual int UserId { get; set; }
        public virtual int TerritoryId { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("UserTerritory = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", UserId: {0}", UserId);
            sb.AppendFormat(", TerritoryId: {0}", TerritoryId);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.Append("]");
            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}