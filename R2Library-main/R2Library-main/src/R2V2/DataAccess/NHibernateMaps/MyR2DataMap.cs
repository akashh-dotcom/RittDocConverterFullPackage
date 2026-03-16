#region

using R2V2.Core.MyR2;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class MyR2DataMap : BaseMap<MyR2Data>
    {
        public MyR2DataMap()
        {
            Table("tMyR2Data");

            Id(x => x.Id, "iMyR2DataId").GeneratedBy.Identity();
            Map(x => x.GuidCookieValue, "vchGuidCookieValue");
            Map(x => x.Type, "iMyR2Type");
            Map(x => x.FolderName, "vchFolderName");
            Map(x => x.DefaultFolder, "tiDefaultFolder");
            Map(x => x.InstitutionId, "iInstitutionId");
            Map(x => x.Json, "vchJson");
        }
    }
}