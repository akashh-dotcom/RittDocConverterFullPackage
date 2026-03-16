#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class InstitutionResourceLicenseMap : BaseMap<InstitutionResourceLicense>
    {
        public InstitutionResourceLicenseMap()
        {
            Table("tInstitutionResourceLicense");

            // iInstitutionResourceLicenseId, iInstitutionId, iResourceId, iLicenseCount, tiLicenseTypeId, tiLicenseOriginalSourceId, dtFirstPurchaseDate
            Id(x => x.Id).Column("iInstitutionResourceLicenseId").GeneratedBy.Identity();
            Map(x => x.InstitutionId).Column("iInstitutionId");
            Map(x => x.ResourceId).Column("iResourceId");
            Map(x => x.LicenseCount).Column("iLicenseCount");
            Map(x => x.LicenseTypeId).Column("tiLicenseTypeId");
            Map(x => x.OriginalSourceId).Column("tiLicenseOriginalSourceId");
            Map(x => x.FirstPurchaseDate).Column("dtFirstPurchaseDate");

            // dtPdaAddedDate, dtPdaAddedToCartDate, vchPdaAddedToCartById, iPdaViewCount, iPdaMaxViews
            Map(x => x.PdaAddedDate).Column("dtPdaAddedDate");
            Map(x => x.PdaAddedToCartDate).Column("dtPdaAddedToCartDate");
            Map(x => x.PdaAddedToCartById).Column("vchPdaAddedToCartById");
            Map(x => x.PdaViewCount).Column("iPdaViewCount");
            Map(x => x.PdaMaxViews).Column("iPdaMaxViews");

            Map(x => x.PdaDeletedDate).Column("dtPdaDeletedDate");
            Map(x => x.PdaDeletedById).Column("vchPdaDeletedById");

            Map(x => x.AveragePrice).Column("decAveragePrice");

            Map(x => x.PdaCartDeletedDate).Column("dtPdaCartDeletedDate");
            Map(x => x.PdaCartDeletedById).Column("iPdaCartDeletedById");
            Map(x => x.PdaCartDeletedByName).Column("vchPdaCartDeletedByName");

            Map(x => x.PdaRuleAddedDate).Column("dtPdaRuleDateAdded");
            Map(x => x.PdaRuleId).Column("iPdaRuleId");

            Map(x => x.BatchId).Column("guidBatchId");

            // dtCreationDate, vchCreatorId, dtLastUpdate, vchUpdaterId, tiRecordStatus
        }
    }
}