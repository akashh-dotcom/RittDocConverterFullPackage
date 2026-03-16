#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Authentication;

#endregion

namespace R2V2.Web.Models.Profile
{
    public class UserEmailOptions : BaseModel, IUserEmailOptions
    {
        public UserEmailOptions()
        {
        }

        public UserEmailOptions(AuthenticatedInstitution institution)
        {
            var user = institution.User;
            EmailOptions = new List<UserEmailOption>();
            foreach (var optionValue in user.OptionValues.Where(x => x.Option.Type.Id == (int)OptionTypeCode.EMAIL))
            {
                if (optionValue.DoNotIncludeOption(institution.ExpertReviewerUserEnabled))
                {
                    continue;
                }

                EmailOptions.Add(new UserEmailOption(optionValue));
            }

            ActionName = "Email";
            ControllerName = "Profile";
            IsSelf = true;
            UserId = user.Id;
        }

        public int UserId { get; set; }
        public List<UserEmailOption> EmailOptions { get; set; }

        public string SubscribedOptions { get; set; }
        public string UnSubscribedOptions { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public bool IsSelf { get; set; }

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
    }
}