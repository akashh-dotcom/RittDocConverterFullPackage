#region

using R2V2.Core.Admin;
using R2V2.Core.Authentication;

#endregion

namespace R2V2.Web.Areas.Admin.Models.User
{
    public class InstitutionUserEdit : AdminBaseModel
    {
        public InstitutionUserEdit()
        {
        }

        public InstitutionUserEdit(AdminInstitution institution) : base(institution)
        {
        }

        public UserEdit User { get; set; }

        public UserQuery UserQuery { get; set; }

        public string UrlReferrer { get; set; }

        public bool IsSelf { get; set; }

        public bool IsExpertReviewerEnabled { get; set; }
    }
}