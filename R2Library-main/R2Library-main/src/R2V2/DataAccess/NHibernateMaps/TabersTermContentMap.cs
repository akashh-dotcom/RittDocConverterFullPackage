#region

using R2V2.Core.Tabers;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class TabersTermContentMap : BaseMap<TermContent>
    {
        public TabersTermContentMap()
        {
            Table("TabersTermContent");

            Id(x => x.TermContentKey).Column("TermContentKey").GeneratedBy.Identity();
            Map(x => x.Term).Column("Term");
            Map(x => x.Content).Column("Content");
            Map(x => x.SectionId).Column("SectionId");
        }
    }
}