#region

using System.Text;
using R2V2.Core.SuperType;
using R2V2.Core.Tabers;

#endregion

namespace R2V2.Core.Resource
{
    public class DictionaryTerm : AuditableEntity, ITermContent, ISoftDeletable, IDebugInfo
    {
        public virtual int ResourceId { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("DictionaryTerm = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Term: {0}, SectionId: {1}, Content: {2}", Term, SectionId, Content);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.Append("]");
            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }
        public virtual string Term { get; set; }
        public virtual string Content { get; set; }
        public virtual string SectionId { get; set; }
    }
}