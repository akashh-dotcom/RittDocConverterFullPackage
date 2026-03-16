#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class PublisherUserMap : BaseMap<PublisherUser>
    {
        public PublisherUserMap()
        {
            Table("tPublisherUser");

            Id(x => x.Id).Column("iPublisherUserId").GeneratedBy.Identity();
            Map(x => x.UserName).Column("vchPublisherUserName");
            Map(x => x.Password).Column("vchPublisherPwd");

            References(x => x.Role).Column("iRoleId");

            References(x => x.Publisher).Column("iPublisherId");
        }
    }
}