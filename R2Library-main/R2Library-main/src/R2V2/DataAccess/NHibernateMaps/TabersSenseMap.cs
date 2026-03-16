#region

using R2V2.Core.Tabers;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class TabersSenseMap : BaseMap<Sense>
    {
        public TabersSenseMap()
        {
            Table("TabersSense");

            Id(x => x.SenseKey).Column("SenseKey").GeneratedBy.Identity();
            //References(x => x.MainEntry).Column("MainEntryKey");
            Map(x => x.Definition).Column("Definition");
        }
    }
}