#region

using Newtonsoft.Json;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource.Discipline
{
    public class Specialty : AuditableEntity, ISoftDeletable, ISpecialty
    {
        public virtual bool RecordStatus { get; set; }
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public virtual int SequenceNumber { get; set; }

        public virtual string ToDebugString()
        {
            return $"Specialty = {ToJsonString()}";
        }

        public virtual string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}