#region

using System.Collections.Generic;
using R2V2.Core.Resource.PracticeArea;

#endregion

namespace R2V2.Web.Models.AlphaIndex
{
    public class AlphaIndex : BaseModel
    {
        public IEnumerable<IPracticeArea> PracticeAreas { get; set; }
        public SpecialtySummaries SpecialtySummaries { get; set; }
    }
}