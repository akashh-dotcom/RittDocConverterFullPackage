#region

using R2V2.Core.Search;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class SearchTypeaheadMap : BaseMap<SearchTypeahead>
    {
        public SearchTypeaheadMap()
        {
            Table("tSearchTypeahead");

            Id(x => x.Id).Column("iSearchTypeaheadId").GeneratedBy.Identity();

            Map(x => x.SearchTerm).Column("vchSearchTerm");
            Map(x => x.Rank).Column("iRank");
            Map(x => x.CreatorId).Column("vchCreatorId");
            Map(x => x.CreationDate).Column("dtCreationDate");
            Map(x => x.UpdaterId).Column("vchUpdaterId");
            Map(x => x.UpdateDate).Column("dtUpdateDate");
            Map(x => x.RecordStatus).Column("tiRecordStatus");
        }
    }
}