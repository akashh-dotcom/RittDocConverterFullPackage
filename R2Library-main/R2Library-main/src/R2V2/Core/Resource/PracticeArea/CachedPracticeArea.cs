#region

using System;
using Newtonsoft.Json;

#endregion

namespace R2V2.Core.Resource.PracticeArea
{
    [Serializable]
    public class CachedPracticeArea : IPracticeArea
    {
        public CachedPracticeArea(IPracticeArea practiceArea)
        {
            Id = practiceArea.Id;
            Code = practiceArea.Code;
            Name = practiceArea.Name;
            SequenceNumber = practiceArea.SequenceNumber;
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int SequenceNumber { get; set; }

        public string ToDebugString()
        {
            return $"CachedPracticeArea = {ToJsonString()}";
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}