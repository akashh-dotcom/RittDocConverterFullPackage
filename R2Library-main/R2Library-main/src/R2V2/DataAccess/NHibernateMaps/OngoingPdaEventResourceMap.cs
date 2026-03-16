#region

using R2V2.Core.Promotion;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class OngoingPdaEventResourceMap : BaseMap<OngoingPdaEventResource>
    {
        public OngoingPdaEventResourceMap()
        {
            Table("tOngoingPdaEventResource");

            //Id(x => x.Id).Column("iOngoingPdaEventResourceId");
            Id(x => x.Id).Column("iOngoingPdaEventResourceId").GeneratedBy.Identity();
            Map(x => x.ResourceId).Column("iResourceId");
            Map(x => x.Isbn).Column("vchResourceIsbn");
            References(x => x.OngoingPdaEvent).Column("iOngoingPdaEventId");
            //References(x => x.Cart).Column("iCartId");
        }
    }
}