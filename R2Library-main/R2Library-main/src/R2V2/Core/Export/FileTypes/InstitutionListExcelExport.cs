#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Authentication;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class InstitutionListExcelExport : ExcelBase
    {
        public InstitutionListExcelExport(List<InstitutionExport> institutionExportList)
        {
            SpecifyColumn("Status", "String");
            SpecifyColumn("Account #", "String");
            SpecifyColumn("Institution Name", "String");
            SpecifyColumn("Address 1", "String");
            SpecifyColumn("Address 2", "String");
            SpecifyColumn("City", "String");
            SpecifyColumn("State", "String");
            SpecifyColumn("Zip", "String");
            SpecifyColumn("Territory", "String");
            SpecifyColumn("Institution Type", "String");

            SpecifyColumn("Admin First Name", "String");
            SpecifyColumn("Admin Last Name", "String");
            SpecifyColumn("Admin Email", "String");
            SpecifyColumn("Other Admin Emails", "String");


            foreach (var institutionWithMainAdminUser in institutionExportList)
            {
                var institution = institutionWithMainAdminUser.Institution;
                var adminUser = institutionWithMainAdminUser.MainAdmin;
                var adminUserString =
                    string.Join(";", institutionWithMainAdminUser.Admins.Select(x => x.Email).ToList());


                PopulateFirstColumn(institution.AccountStatus.Description);
                PopulateNextColumn(institution.AccountNumber);
                PopulateNextColumn(institution.Name);
                PopulateNextColumn(institution.Address.Address1);
                PopulateNextColumn(institution.Address.Address2);
                PopulateNextColumn(institution.Address.City);
                PopulateNextColumn(institution.Address.State);
                PopulateNextColumn(institution.Address.Zip);
                PopulateNextColumn(institution.Territory != null ? institution.Territory.Name : "");
                PopulateNextColumn(institution.Type != null ? institution.Type.Name : "");
                PopulateNextColumn(adminUser != null ? adminUser.FirstName : "");
                PopulateNextColumn(adminUser != null ? adminUser.LastName : "");
                PopulateNextColumn(adminUser != null ? adminUser.Email : "");
                PopulateLastColumn(adminUserString);
            }
        }


        public InstitutionListExcelExport(Dictionary<Institution.Institution, User> institutionsWithMainAdminUser)
        {
            SpecifyColumn("Status", "String");
            SpecifyColumn("Account #", "String");
            SpecifyColumn("Institution Name", "String");
            SpecifyColumn("Address 1", "String");
            SpecifyColumn("Address 2", "String");
            SpecifyColumn("City", "String");
            SpecifyColumn("State", "String");
            SpecifyColumn("Zip", "String");
            SpecifyColumn("Territory", "String");

            SpecifyColumn("Admin First Name", "String");
            SpecifyColumn("Admin Last Name", "String");
            SpecifyColumn("Admin Email", "String");

            foreach (var institutionWithMainAdminUser in institutionsWithMainAdminUser)
            {
                var institution = institutionWithMainAdminUser.Key;
                var adminUser = institutionWithMainAdminUser.Value;

                PopulateFirstColumn(institution.AccountStatus.Description);
                PopulateNextColumn(institution.AccountNumber);
                PopulateNextColumn(institution.Name);
                PopulateNextColumn(institution.Address.Address1);
                PopulateNextColumn(institution.Address.Address2);
                PopulateNextColumn(institution.Address.City);
                PopulateNextColumn(institution.Address.State);
                PopulateNextColumn(institution.Address.Zip);
                PopulateNextColumn(institution.Territory != null ? institution.Territory.Name : "");
                PopulateNextColumn(adminUser != null ? adminUser.FirstName : "");
                PopulateNextColumn(adminUser != null ? adminUser.LastName : "");
                PopulateLastColumn(adminUser != null ? adminUser.Email : "");
            }
        }
    }
}