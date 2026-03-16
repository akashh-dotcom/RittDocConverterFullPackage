#region

using R2V2.Core.Resource.Topic;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class KeywordMap : BaseMap<Keyword>
    {
        public KeywordMap()
        {
            Table("tKeyword");

            Id(x => x.Id, "iKeywordId").GeneratedBy.Identity();
            Map(x => x.Description, "vchKeywordDesc");

            HasMany(x => x.KeywordResources).KeyColumn("iKeywordId");
        }
    }
}