#region

using System.Collections.Generic;
using System.Web.Mvc;
using R2V2.Core.Authentication;
using R2V2.Web.Areas.Admin.Models.Institution;
using R2V2.Web.Areas.Admin.Models.User;

#endregion

namespace R2V2.Web.Areas.Admin.Models.TrialInstitution
{
    public class TrialInstitution : AdminBaseModel
    {
        public TrialInstitution()
        {
        }

        public TrialInstitution(bool preludeCustomerFound)
        {
            PreludeCustomerNotFound = !preludeCustomerFound;
        }

        public bool PreludeCustomerNotFound { get; set; }
        public bool IsTestMode { get; set; }
        public InstitutionEditViewModel InstitutionTrial { get; set; }
        public UserEdit User { get; set; }

        public void PopulateSelectLists(List<Department> departments)
        {
            //------------------------START User Select List START-------------------------------//
            var items = new List<Department> { new Department { Id = 0, Name = "Enter Custom Department" } };
            items.AddRange(departments);
            User.DepartmentSelectList = new SelectList(items, "Id", "Name");

            if (items.Contains(User.Department) || User.Department == null)
            {
                return;
            }

            User.CustomDepartment = User.Department.Name;
            User.CustomDepartmentId = User.Department.Id;
            //------------------------END User Select List END-------------------------------//
        }
    }
}