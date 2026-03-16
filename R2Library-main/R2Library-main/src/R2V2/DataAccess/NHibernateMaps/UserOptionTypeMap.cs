#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserOptionTypeMap : BaseMap<UserOptionType>
    {
        public UserOptionTypeMap()
        {
            Table("dbo.tUserOptionType");
            Id(x => x.Id, "iUserOptionTypeId").GeneratedBy.Identity();
            Map(x => x.Code, "vchUserOptionTypeCode");
            Map(x => x.Description, "vchUserOptionTypeDescription");
        }
    }
}