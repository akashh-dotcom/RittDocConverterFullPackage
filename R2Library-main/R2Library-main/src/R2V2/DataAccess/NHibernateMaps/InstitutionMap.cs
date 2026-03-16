#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class InstitutionMap : BaseMap<Institution>
    {
        public InstitutionMap()
        {
            Table("dbo.tInstitution");

            Id(x => x.Id, "iInstitutionId").GeneratedBy.Identity();
            Map(x => x.Name, "vchInstitutionName");
            Map(x => x.NameKey)
                .Formula(
                    "ltrim(rtrim(substring(vchInstitutionName, 0, 2)))"); // Added for alphabetical pagination since it's difficult to group by an aggregate function.
            Map(x => x.AccountNumber, "vchInstitutionAcctNum");
            Component(x => x.Address, a =>
            {
                a.Map(x => x.Address1, "vchInstitutionAddr1");
                a.Map(x => x.Address2, "vchInstitutionAddr2");
                a.Map(x => x.City, "vchInstitutionCity");
                a.Map(x => x.State, "vchInstitutionState");
                a.Map(x => x.Zip, "vchInstitutionZip");
            });
            Map(x => x.Phone, "vchInstitutionContactPhone");
            Component(x => x.Trial, t =>
            {
                t.Map(x => x.StartDate, "dtTrialAcctStart");
                t.Map(x => x.EndDate, "dtTrialAcctEnd");
                t.Map(x => x.EmailWarningDate, "dtTrialEndEmailWarn");
                t.Map(x => x.Email3DayWarningDate, "dtTrialEndEmail3DayWarn");
                t.Map(x => x.EmailFinalDate, "dtTrialEndEmailFinal");
            });
            Map(x => x.DisplayAllProducts, "tiDisplayPPV");
            Map(x => x.EULASigned, "tiEULASigned");
            Map(x => x.PdaEulaSigned, "tiPdaEulaSigned");
            Map(x => x.Discount, "decInstDiscount");
            Component(x => x.AnnualFee, f =>
            {
                f.Map(x => x.FeeDate, "dtAnnualFee");
                f.Map(x => x.PO, "vchAnnualFeePO");
                f.Map(x => x.PayType, "iAnnualFeePaytype");
            });
            Map(x => x.HouseAccount, "tiHouseAcct");
            Map(x => x.AthensOrgId).Column("vchAthensOrgId").CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.AthensAffiliation).Column("vchAthensScopedAffiliation").CustomType("StringClob")
                .CustomSqlType("nvarchar(max)");

            Map(x => x.LogUrl, "vchLogUrl");
            Map(x => x.HomePageId, "tiHomePage");
            Map(x => x.TrustedKey, "vchTrustedKey");

            HasMany(x => x.InstitutionResourceLicenses).KeyColumn("iInstitutionId").AsBag().ReadOnly().Cascade.None();

            Map(x => x.AccountStatusId, "iInstitutionAcctStatusId");
            Map(x => x.AccessTypeId, "iAccessTypeId");
            Map(x => x.ExpertReviewerUserEnabled, "tiFacultyUserEnabled");
            Map(x => x.IncludeArchivedTitlesByDefault, "tiIncludeArchivedTitlesByDefault");

            HasMany(x => x.InstitutionBrandings).KeyColumn("iInstitutionId").AsBag().ReadOnly();
            HasMany(x => x.ReserveShelves).KeyColumn("iInstitutionId").AsBag().ReadOnly();

            HasMany(x => x.ProductSubscriptions).KeyColumn("iInstitutionId").AsBag().Cascade.AllDeleteOrphan();

            References(x => x.Territory).Column("iTerritoryId").Cascade.None();

            Map(x => x.ProxyPrefix, "vchProxyPrefix");
            References(x => x.Type).Column("iInstitutionTypeId").Cascade.None();
            Map(x => x.OclcSymbol, "vchOclcSymbol");


            Map(x => x.EnableIpPlus, "tiEnableIpPlus");
            Map(x => x.EnableHomePageCollectionLink, "tiHomePageCollectionLink");
        }
    }
}