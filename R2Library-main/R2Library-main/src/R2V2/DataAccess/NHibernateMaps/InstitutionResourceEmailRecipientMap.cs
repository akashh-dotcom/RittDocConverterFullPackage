#region

using R2V2.Core.Email;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class InstitutionResourceEmailRecipientMap : BaseMap<InstitutionResourceEmailRecipient>
    {
        public InstitutionResourceEmailRecipientMap()
        {
            Table("tInstitutionResourceEmailRecipient");

            Id(x => x.Id).Column("iInstitutionResourceEmailRecipientId").GeneratedBy.Identity();
            Map(x => x.EmailAddress).Column("vchEmailAddress");
            Map(x => x.AddressType).Column("vchAddressType");
            References(x => x.InstitutionResourceEmail).Column("iInstitutionResourceEmailId");
        }
    }
}