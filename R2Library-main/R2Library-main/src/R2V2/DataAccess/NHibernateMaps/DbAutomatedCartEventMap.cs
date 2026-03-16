#region

using FluentNHibernate.Mapping;
using R2V2.Core.AutomatedCart;
using R2V2.Core.Reports;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class DbAutomatedCartEventMap : ClassMap<DbAutomatedCartEvent>
    {
        public DbAutomatedCartEventMap()
        {
            Table("vAutomatedCartEvent");
            Id(x => x.Id).Column("iAutomatedCartEventId").GeneratedBy.Guid();
            Map(x => x.InstitutionId).Column("iInstitutionId");
            Map(x => x.ResourceId).Column("iResourceId");
            Map(x => x.TerritoryId).Column("iTerritoryId");
            Map(x => x.EventDate).Column("eventDate");
            Map(x => x.NewEdition).Column("NewEdition");
            Map(x => x.TriggeredPda).Column("TriggeredPDA");
            Map(x => x.Turnaway).Column("Turnaway");
            Map(x => x.Reviewed).Column("Reviewed");
            Map(x => x.Requested).Column("Requested");
        }
    }

    public sealed class DbAutomatedCartMap : BaseMap<DbAutomatedCart>
    {
        public DbAutomatedCartMap()
        {
            Table("tAutomatedCart");
            Id(x => x.Id).Column("iAutomatedCartId").GeneratedBy.Identity();
            Map(x => x.Period).Column("iPeriod").CustomType<ReportPeriod>();
            Map(x => x.StartDate).Column("dtStartDate");
            Map(x => x.EndDate).Column("dtEndDate");
            Map(x => x.NewEdition).Column("tiNewEdition");
            Map(x => x.TriggeredPda).Column("tiPda");
            Map(x => x.Reviewed).Column("tiReviewed");
            Map(x => x.Turnaway).Column("tiTurnaway");
            Map(x => x.Requested).Column("tiRequested");
            Map(x => x.Discount).Column("decDiscount");
            Map(x => x.AccountNumbers).Column("vchAccountNumbers").CustomType("StringClob")
                .CustomSqlType("nvarchar(max)");
            Map(x => x.EmailSubject).Column("vchEmailSubject");
            Map(x => x.EmailTitle).Column("vchEmailTitle");
            Map(x => x.EmailText).Column("vchEmailText").CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.CartName).Column("vchCartName");
            Map(x => x.TerritoryIds).Column("vchTerritoryIds");
            Map(x => x.InstitutionTypeIds).Column("vchInstitutionTypeIds");
        }
    }

    public sealed class DbAutomatedCartInstitutionMap : BaseMap<DbAutomatedCartInstitution>
    {
        public DbAutomatedCartInstitutionMap()
        {
            Table("tAutomatedCartInstitution");
            Id(x => x.Id).Column("iAutomatedCartInstitutionId").GeneratedBy.Identity();
            Map(x => x.AutomatedCartId).Column("iAutomatedCartId");
            Map(x => x.InstitutionId).Column("iInstitutionId");
            Map(x => x.CartId).Column("iCartId");
            Map(x => x.EmailsSent).Column("iEmailsSent");
            //EmailsSent
        }
    }

    public sealed class DbAutomatedCartResourceMap : BaseMap<DbAutomatedCartResource>
    {
        public DbAutomatedCartResourceMap()
        {
            Table("tAutomatedCartResource");
            Id(x => x.Id).Column("iAutomatedCartResourceId").GeneratedBy.Identity();
            Map(x => x.AutomatedCartInstitutionId).Column("iAutomatedCartInstitutionId");
            Map(x => x.ResourceId).Column("iResourceId");
            Map(x => x.CartItemId).Column("iCartItemId");
            Map(x => x.ListPrice).Column("decListPrice");
            Map(x => x.DiscountPrice).Column("decDiscountPrice");
            Map(x => x.NewEditionCount).Column("iNewEdition");
            Map(x => x.TriggeredPdaCount).Column("iPda");
            Map(x => x.ReviewedCount).Column("iReviewed");
            Map(x => x.TurnawayCount).Column("iTurnaway");
            Map(x => x.RequestedCount).Column("iRequested");
        }
    }
}