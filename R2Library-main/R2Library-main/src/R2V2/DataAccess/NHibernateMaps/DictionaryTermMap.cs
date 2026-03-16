#region

using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class DictionaryTermMap : BaseMap<DictionaryTerm>
    {
        public DictionaryTermMap()
        {
            Table("tDictionaryTerm");

            Id(x => x.Id).Column("iDictionaryTermId").GeneratedBy.Identity();
            Map(x => x.Term).Column("vchTerm");
            //Map(x => x.Content).Column("vchContent");
            Map(x => x.Content).Column("vchContent").CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.SectionId).Column("vchSectionId");
            Map(x => x.ResourceId).Column("iDictionaryResourceId");
        }
    }
}