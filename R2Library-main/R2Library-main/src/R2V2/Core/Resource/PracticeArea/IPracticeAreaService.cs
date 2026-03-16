#region

using System.Collections.Generic;

#endregion

namespace R2V2.Core.Resource.PracticeArea
{
    public interface IPracticeAreaService
    {
        IEnumerable<IPracticeArea> GetAllPracticeAreas();
        IPracticeArea GetPracticeAreaById(int practiceAreaId);
        IPracticeArea GetPracticeAreaById(string practiceAreaId);

        PracticeArea GetPracticeArea(int practiceAreaId);

        void ClearCache();
    }
}