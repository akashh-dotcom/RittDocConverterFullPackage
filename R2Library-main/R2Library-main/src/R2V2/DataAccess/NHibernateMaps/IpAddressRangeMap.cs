#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class IpAddressRangeMap : BaseMap<IpAddressRange>
    {
        public IpAddressRangeMap()
        {
            Table("dbo.tIpAddressRange");
            Id(x => x.Id, "iIpAddressId").GeneratedBy.Identity();
            References(x => x.Institution).Column("iInstitutionId").ReadOnly();
            Map(x => x.InstitutionId, "iInstitutionId");

            Map(x => x.OctetA, "tiOctetA");
            Map(x => x.OctetB, "tiOctetB");
            Map(x => x.OctetCStart, "tiOctetCStart");
            Map(x => x.OctetCEnd, "tiOctetCEnd");
            Map(x => x.OctetDStart, "tiOctetDStart");
            Map(x => x.OctetDEnd, "tiOctetDEnd");
            Map(x => x.IpNumberStart, "iDecimalStart");
            Map(x => x.IpNumberEnd, "iDecimalEnd");
            Map(x => x.Description, "vchDescription");
        }
    }
}