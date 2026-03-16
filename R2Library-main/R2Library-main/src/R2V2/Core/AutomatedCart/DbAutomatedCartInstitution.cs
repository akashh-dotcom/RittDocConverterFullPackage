#region

using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.AutomatedCart
{
    public class DbAutomatedCartInstitution : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual int AutomatedCartId { get; set; }
        public virtual int InstitutionId { get; set; }
        public virtual int? CartId { get; set; }
        public virtual int? EmailsSent { get; set; }

        //public virtual Cart Cart { get; set; }
        public virtual string ToDebugString()
        {
            return new StringBuilder()
                .Append("DbAutomatedCartInstitution=[")
                .Append($"AutomatedCartId: {AutomatedCartId},")
                .Append($"InstitutionId: {InstitutionId},")
                .Append($"CartId: {CartId},")
                .Append($"RecordStatus: {RecordStatus}]")
                .ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}