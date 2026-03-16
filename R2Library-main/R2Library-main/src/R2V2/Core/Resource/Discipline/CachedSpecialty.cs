#region

using System;
using Newtonsoft.Json;

#endregion

namespace R2V2.Core.Resource.Discipline
{
    [Serializable]
    public class CachedSpecialty : ISpecialty
    {
        public CachedSpecialty(ISpecialty specialty)
        {
            Id = specialty.Id;
            Code = specialty.Code;
            Name = specialty.Name;
            SequenceNumber = specialty.SequenceNumber;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int SequenceNumber { get; set; }

        public string ToDebugString()
        {
            return $"CachedSpecialty = {ToJsonString()}";
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}