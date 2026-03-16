#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Review
{
    public class ReviewUser
    {
        public ReviewUser()
        {
        }

        public ReviewUser(Core.Recommendations.ReviewUser reviewUser)
        {
            Id = reviewUser.Id;
            SetUser(reviewUser.User);
            IsSelected = Id > 0;
        }

        public ReviewUser(Core.Authentication.User user)
        {
            SetUser(user);
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsSelected { get; set; }

        private void SetUser(IUser user)
        {
            UserId = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
        }
    }
}