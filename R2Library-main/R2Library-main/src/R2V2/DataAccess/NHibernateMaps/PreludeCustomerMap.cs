#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class PreludeCustomerMap : BaseMap<PreludeCustomer>
    {
        public PreludeCustomerMap()
        {
            Table("dbo.vPreludeCustomer");
            Id(x => x.Id, "vchAccountNumber").GeneratedBy.Identity();

            Map(x => x.Name, "vchAccountName");
            Map(x => x.AccountNumber, "vchAccountNumber").ReadOnly();

            Component(x => x.Address, a =>
            {
                a.Map(x => x.Address1, "vchBillToAddress1");
                a.Map(x => x.Address2, "vchBillToAddress2");
                a.Map(x => x.City, "vchBillToCity");
                a.Map(x => x.State, "vchBillToState");
                a.Map(x => x.Zip, "vchBillToZip");
            });

            Map(x => x.AdministratorEmail, "vchEmailAddress");

            Map(x => x.Territory, "vchTerritory");
            Map(x => x.TypeName, "vchTypeName");
        }
    }
}