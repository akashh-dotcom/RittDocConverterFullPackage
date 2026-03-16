#region

using System.Collections.Generic;
using R2V2.Core.Publisher;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;

#endregion

namespace R2V2.Web.Models.Browse
{
    public class Browse : BaseModel
    {
        //_defaultState: { 'include': 1, 'type': 'publications' },

        public bool DisplayTocAvailable { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<PracticeArea> PracticeAreas { get; set; }

        public IEnumerable<IPublisher> Publishers { get; set; }
        public IEnumerable<Specialty> Disciplines { get; set; }

        public int DefaultInclude { get; set; } = 1;
        public string DefaultType { get; set; } = "publications";

        public bool EnableCollectionLink { get; set; }
        public string CollectionLinkName { get; set; }
    }
}