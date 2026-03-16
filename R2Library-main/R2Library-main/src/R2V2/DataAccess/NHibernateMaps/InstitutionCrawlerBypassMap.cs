#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class InstitutionCrawlerBypassMap : BaseMap<InstitutionCrawlerBypass>
    {
        public InstitutionCrawlerBypassMap()
        {
            //SELECT iInstitutionCrawlerBypassId, iInstitutionId, tiOctetA, tiOctetB, tiOctetC, tiOctetD
            //, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus, vchUserAgent
            //FROM tInstitutionCrawlerBypass

            Table("dbo.tInstitutionCrawlerBypass");
            Id(x => x.Id, "iInstitutionCrawlerBypassId").GeneratedBy.Identity();
            References(x => x.Institution).Column("iInstitutionId").ReadOnly();
            Map(x => x.InstitutionId, "iInstitutionId");

            Map(x => x.OctetA, "tiOctetA");
            Map(x => x.OctetB, "tiOctetB");
            Map(x => x.OctetC, "tiOctetC");
            Map(x => x.OctetD, "tiOctetD");
            Map(x => x.IpNumber, "iDecimal");
            Map(x => x.UserAgent, "vchUserAgent");
        }
    }
}