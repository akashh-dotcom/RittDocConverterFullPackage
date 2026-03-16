#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Recommendations
{
    public class RecommendedUser
    {
        public RecommendedUser()
        {
        }

        public RecommendedUser(IUser user)
        {
            if (user == null)
            {
                return;
            }

            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Department = user.Department == null ? null : user.Department.Name;
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Department { get; set; }
    }
}