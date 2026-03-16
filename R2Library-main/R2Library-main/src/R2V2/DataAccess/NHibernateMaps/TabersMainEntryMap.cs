#region

using R2V2.Core.Tabers;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class TabersMainEntryMap : BaseMap<MainEntry>
    {
        public TabersMainEntryMap()
        {
            Table("TabersMainEntry");

            Id(x => x.MainEntryKey).Column("MainEntryKey").GeneratedBy.Identity();
            HasMany(x => x.Senses).KeyColumn("MainEntryKey");
            Map(x => x.Name).Column("Name");
        }
    }
}