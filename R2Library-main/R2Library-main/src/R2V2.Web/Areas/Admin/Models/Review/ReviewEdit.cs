#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Review
{
    public class ReviewEdit : AdminBaseModel
    {
        /// <summary>
        /// </summary>
        public ReviewEdit()
        {
        }

        /// <summary>
        /// </summary>
        public ReviewEdit(IAdminInstitution institution, Core.Recommendations.Review review,
            List<Core.Authentication.User> users, string specialIconBaseUrl)
            : base(institution)
        {
            Review = review.ToReview();
            ExpertReviewers = new List<ReviewUser>();
            foreach (var user in users)
            {
                var u = review == null || review.ReviewUsers == null
                    ? null
                    : review.ReviewUsers.FirstOrDefault(x => x.UserId == user.Id);
                ExpertReviewers.Add(u == null ? new ReviewUser(user) : new ReviewUser(u));
            }

            SpecialIconBaseUrl = specialIconBaseUrl;
        }

        public ReviewModel Review { get; set; }
        public IList<ReviewUser> ExpertReviewers { get; set; }
        public int[] SelectedExpertReviewerUserIds { get; set; }

        public CollectionManagementQuery CollectionManagementQuery { get; set; }
        public string DoodyReviewUrl { get; set; }
        public bool IsPdaEnabled { get; set; }
        public string ActionType { get; set; }
        public string ActionResultsMessage { get; set; }
        public string SpecialIconBaseUrl { get; set; }
    }
}