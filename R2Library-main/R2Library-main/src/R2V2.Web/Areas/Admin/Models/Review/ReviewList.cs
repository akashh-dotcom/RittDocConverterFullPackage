#region

using System.Collections.Generic;
using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Review
{
    public class ReviewList : AdminBaseModel
    {
        public ReviewList()
        {
        }

        public ReviewList(IAdminInstitution institution, List<ReviewModel> reviewLists)
            : base(institution)
        {
            Reviews = reviewLists;
        }

        public List<ReviewModel> Reviews { get; set; }
    }
}