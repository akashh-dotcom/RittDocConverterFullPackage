#region

using FluentNHibernate.Mapping;
using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ResourceFileMap : ClassMap<ResourceFile>
    {
        public ResourceFileMap()
        {
            // select iResourceFileId, iResourceId, vchFileNameFull, vchFileNamePart1, vchFileNamePart3, iDocumentId from tResourceFile

            Table("tResourceFile");
            Id(x => x.Id).Column("iResourceFileId").GeneratedBy.Identity();

            Map(x => x.ResourceId).ReadOnly().Column("iResourceId");
            Map(x => x.FilenameFull).Column("vchFileNameFull");
            Map(x => x.FilenamePart1).Column("vchFileNamePart1");
            Map(x => x.FilenamePart3).Column("vchFileNamePart3");
            Map(x => x.DocumentId).Column("iDocumentId");

            References(x => x.Resource).Column("iResourceId").ReadOnly().Cascade.None();
        }
    }
}