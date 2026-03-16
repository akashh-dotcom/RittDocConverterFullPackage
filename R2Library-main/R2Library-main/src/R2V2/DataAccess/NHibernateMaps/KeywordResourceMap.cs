#region

using R2V2.Core.Resource.Topic;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class KeywordResourceMap : BaseMap<KeywordResource>
    {
        public KeywordResourceMap()
        {
            Table("tKeywordResource");

            Id(x => x.Id, "iKeywordResourceId").GeneratedBy.Identity();
            Map(x => x.KeywordId, "iKeywordId");
            Map(x => x.Isbn, "vchResourceISBN");
            Map(x => x.ChapterId, "vchChapterId");
            Map(x => x.SectionId, "vchSectionId");

            HasMany(x => x.InstitutionResourceLicenses).KeyColumn("vchResourceISBN").Inverse().Cascade.None();
        }
    }
}