#region

using System;

#endregion

namespace R2V2.Core.Territory
{
    [Serializable]
    public class CachedTerritory : ITerritory
    {
        public CachedTerritory(ITerritory territory)
        {
            Id = territory.Id;
            Name = territory.Name;
            Code = territory.Code;
            RecordStatus = territory.RecordStatus;
        }

        public bool RecordStatus { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
    }
}