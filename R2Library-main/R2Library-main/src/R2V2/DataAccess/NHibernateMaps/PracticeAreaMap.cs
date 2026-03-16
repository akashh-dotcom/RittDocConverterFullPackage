#region

using R2V2.Core.Resource.PracticeArea;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class PracticeAreaMap : BaseMap<PracticeArea>
    {
        public PracticeAreaMap()
        {
            Table("tPracticeArea");

            Id(x => x.Id).Column("iPracticeAreaId").GeneratedBy.Identity();
            Map(x => x.Name).Column("vchPracticeAreaName");
            Map(x => x.Code).Column("vchPracticeAreaCode");
            Map(x => x.SequenceNumber).Column("iSequenceNumber");
        }
    }
}