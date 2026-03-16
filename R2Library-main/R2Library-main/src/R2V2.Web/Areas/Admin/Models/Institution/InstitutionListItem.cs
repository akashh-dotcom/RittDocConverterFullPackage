#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Institution
{
    public class InstitutionListItem
    {
        public InstitutionListItem(IInstitution institution)
        {
            AccountStatus = institution.AccountStatus;
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
        }

        public int InstitutionId { get; private set; }
        public string InstitutionName { get; private set; }
        public string AccountNumber { get; private set; }
        public Address Address { get; private set; }
        public IInstitutionAccountStatus AccountStatus { get; private set; }
        public InstitutionTerritory InstitutionTerritory { get; set; }
    }
}