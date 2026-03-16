#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserSubscriptionMap : BaseMap<UserSubscription>
    {
        public UserSubscriptionMap()
        {
            Table("dbo.tSubscriptionUser");
            Id(x => x.Id, "iSubscriptionUserId").GeneratedBy.Identity();
            Map(x => x.SubscriptionId, "iSubscriptionId");
            Map(x => x.ExpirationDate, "dtExpirationDate");
            Map(x => x.TrialExpirationDate, "dtTrialExpirationDate");
            Map(x => x.ActivationDate, "dtActivationDate");
            Map(x => x.UserId, "iUserId");
            Map(x => x.Type, "iSubscriptionType").CustomType<int>();
            References(x => x.Subscription).Column("iSubscriptionId").ReadOnly();
            ;
        }
    }

    public sealed class SubscriptionMap : BaseMap<Subscription>
    {
        public SubscriptionMap()
        {
            Table("dbo.tSubscription");
            Id(x => x.Id, "iSubscriptionId").GeneratedBy.Identity();
            Map(x => x.Name, "vchName");
            Map(x => x.Description, "vchDescription");
            Map(x => x.CmsName, "vchCmsName");
            Map(x => x.MonthlyPrice, "decMonthlyPrice");
            Map(x => x.AnnualPrice, "decAnnualPrice");
            Map(x => x.AllowSubscription, "tiAllowSubscriptions");
            HasMany(x => x.SubResources).KeyColumn("iSubscriptionId").AsBag().ReadOnly();
        }
    }

    public sealed class SubscriptionResourceMap : BaseMap<SubscriptionResource>
    {
        public SubscriptionResourceMap()
        {
            Table("dbo.tSubscriptionResource");
            Id(x => x.Id, "iSubscriptionResourceId").GeneratedBy.Identity();
            Map(x => x.ResourceId, "iResourceId");
            Map(x => x.SubscriptionId, "iSubscriptionId");
        }
    }

    public sealed class SubscriptionOrderHistoryMap : BaseMap<SubscriptionOrderHistory>
    {
        public SubscriptionOrderHistoryMap()
        {
            Table("dbo.tSubscriptionOrderHistory");
            Id(x => x.Id, "iSubscriptionOrderHistoryId").GeneratedBy.Identity();
            Map(x => x.SubscriptionId, "iSubscriptionId");
            Map(x => x.UserId, "iUserId");
            Map(x => x.SubscriptionUserId, "iSubscriptionUserId");
            Map(x => x.OrderNumber, "vchOrderNumber");
            Map(x => x.PurchaseOrderNumber, "vchPurchaseOrderNumber");
            Map(x => x.PurchaseOrderComment, "vchPurchaseOrderComment");
            Map(x => x.Price, "decPrice");
            Map(x => x.Type, "iSubscriptionType").CustomType<int>();
        }
    }
}