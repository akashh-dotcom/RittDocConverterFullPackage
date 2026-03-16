#region

using System;
using Newtonsoft.Json;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource.PracticeArea
{
    [Serializable]
    public class PracticeArea : AuditableEntity, ISoftDeletable, IPracticeArea
    {
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        public virtual int SequenceNumber { get; set; }

        public virtual string ToDebugString()
        {
            return $"PracticeArea = {ToJsonString()}";
        }

        public virtual bool RecordStatus { get; set; }

        public virtual string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}