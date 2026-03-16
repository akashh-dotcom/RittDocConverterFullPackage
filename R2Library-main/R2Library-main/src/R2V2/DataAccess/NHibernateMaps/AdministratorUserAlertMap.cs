#region

using R2V2.Core.Admin;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class AdministratorUserAlertMap : BaseMap<UserAlert>
    {
        public AdministratorUserAlertMap()
        {
            Table("tUserAlert");

            Id(x => x.Id).Column("iUserAlertId").GeneratedBy.Identity();
            Map(x => x.UserId).Column("iUserId");
            Map(x => x.PublisherUserId).Column("iPublisherUserId");
            Map(x => x.AlertId).Column("iAlertId");
        }
    }
}