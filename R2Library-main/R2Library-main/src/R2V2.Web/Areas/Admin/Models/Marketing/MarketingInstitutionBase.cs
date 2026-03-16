#region

using R2V2.Core.Institution;
using R2V2.Web.Areas.Admin.Models.Institution;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Marketing
{
    public class MarketingInstitutionBase
    {
        public MarketingInstitutionBase(IInstitution institution)
        {
            InstitutionId = institution.Id;
            InstitutionName = institution.Name;
            AccountNumber = institution.AccountNumber;
            Address = new Address
            {
                Address1 = institution.Address.Address1,
                Address2 = institution.Address.Address2,
                City = institution.Address.City,
                State = institution.Address.State,
                Zip = institution.Address.Zip
            };
            InstitutionTerritory = new InstitutionTerritory(institution.Territory);
            InstitutionType = institution.Type;
        }

        public int InstitutionId { get; }
        public string InstitutionName { get; }
        public string AccountNumber { get; }
        public Address Address { get; }
        public InstitutionTerritory InstitutionTerritory { get; }

        public InstitutionType InstitutionType { get; }
    }
}