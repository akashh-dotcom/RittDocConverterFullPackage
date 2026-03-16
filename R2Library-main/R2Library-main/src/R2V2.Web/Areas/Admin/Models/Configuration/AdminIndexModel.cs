#region

using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.BackEnd
{
    public class AdminIndexModel : AdminBaseModel
    {
        public AdminIndexModel()
        {
        }

        public AdminIndexModel(IAdminInstitution institution, string[] authorizedUsers) : base(institution)
        {
            AuthorizedUsers = authorizedUsers;
        }

        public string[] AuthorizedUsers { get; set; }
    }
}