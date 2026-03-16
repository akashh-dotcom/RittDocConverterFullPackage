#region

using FluentNHibernate.Mapping;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public class SoftDeleteFilter : FilterDefinition
    {
        public SoftDeleteFilter()
        {
            WithName("SoftDeletesFilter");
            WithCondition("tiRecordStatus = 1");
        }
    }
}