#region

using System.Collections.Generic;

#endregion

namespace R2V2.Web.Areas.Admin.Models.SubscriptionManagement
{
    public class SubscriptionUsers : AdminBaseModel
    {
        public IEnumerable<User.User> Users { get; set; }
    }
}