#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;

#endregion

namespace R2V2.Web.Areas.Admin.Models.User
{
    public class AdminUserEmailOptions : AdminBaseModel, IUserEmailOptions
    {
        public AdminUserEmailOptions()
        {
        }

        public AdminUserEmailOptions(IEnumerable<UserOptionValue> optionValues, IAdminInstitution institution,
            IUser user, bool isSelf) : base(institution)
        {
            UserId = user.Id;
            EmailOptions = new List<UserEmailOption>();
            foreach (var optionValue in optionValues.Where(x => x.Option.Type.Id == (int)OptionTypeCode.EMAIL))
            {
                if (optionValue.DoNotIncludeOption(institution.ExpertReviewerUserEnabled))
                {
                    continue;
                }

                EmailOptions.Add(new UserEmailOption(optionValue));
            }

            ActionName = "Email";
            ControllerName = "User";
            IsSelf = isSelf;
            UserDescription = string.Format("{0}{1} ({2})", user.LastName,
                string.IsNullOrWhiteSpace(user.FirstName) ? "" : $", {user.FirstName}", user.Email);
        }

        public string UserDescription { get; set; }
        public int UserId { get; set; }
        public List<UserEmailOption> EmailOptions { get; set; }

        public IEnumerable<UserEmailOption> OtherOptions()
        {
            return EmailOptions.Where(x => x.UserEmailGroupCode == UserEmailGroupCode.Other)
                .OrderBy(x => x.ListPosition);
        }

        public IEnumerable<UserEmailOption> WeeklyOptions()
        {
            return EmailOptions.Where(x => x.UserEmailGroupCode == UserEmailGroupCode.Weekly)
                .OrderBy(x => x.ListPosition);
        }

        public IEnumerable<UserEmailOption> MonthlyOptions()
        {
            return EmailOptions.Where(x => x.UserEmailGroupCode == UserEmailGroupCode.Monthly)
                .OrderBy(x => x.ListPosition);
        }

        public string SubscribedOptions { get; set; }
        public string UnSubscribedOptions { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public bool IsSelf { get; set; }
    }
}