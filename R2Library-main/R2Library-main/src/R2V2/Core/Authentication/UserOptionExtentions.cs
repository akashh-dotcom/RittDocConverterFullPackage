#region

using System.Linq;

#endregion

namespace R2V2.Core.Authentication
{
    public static class UserOptionExtentions
    {
        /// <summary>
        ///     Excludes Expert Reviewer Option if the institution does not have ExpertReviewerUserEnabled
        /// </summary>
        public static bool DoNotIncludeOption(this UserOptionValue optionValue, bool institutionOption)
        {
            if (!institutionOption &&
                (optionValue.Option.Code == UserOptionCode.ExpertReviewRecommend ||
                 optionValue.Option.Code == UserOptionCode.ExpertReviewUserRequest))
            {
                return true;
            }

            return false;
        }

        public static bool UserHasEmailOption(this User user, UserOptionCode optionCode)
        {
            return user.OptionValues.Any(x => x.Option.Code == optionCode && x.Value == "1");
        }

        public static IQueryable<User> HasOptionSelected(this IQueryable<User> users, UserOptionCode optionEnum)
        {
            return users
                .Where(x =>
                    x.OptionValues.Any(y =>
                        y.Value == "1" && y.RecordStatus
                                       && y.Option.Code == optionEnum && y.Option.RecordStatus));
        }

        public static int ToPositition(this UserOptionValue optionValue)
        {
            switch (optionValue.Option.Code)
            {
                //On Demand
                case UserOptionCode.CartRemind:
                case UserOptionCode.AnnualMaintenanceFee:
                    return 1;
                case UserOptionCode.AccessDenied:
                    return 2;
                case UserOptionCode.LibrarianAlert:
                    return 3;
                case UserOptionCode.PdaAddToCart:
                    return 4;
                case UserOptionCode.ExpertReviewRecommend:
                    return 5;
                case UserOptionCode.ExpertReviewUserRequest:
                    return 6;

                //Weekly
                case UserOptionCode.NewResource:
                    return 1;
                case UserOptionCode.NewEdition:
                    return 2;
                case UserOptionCode.ForthcomingPurchase:
                    return 3;
                case UserOptionCode.ArchivedAlert:
                    return 4;
                case UserOptionCode.PdaReport:
                    return 5;

                //Monthly
                case UserOptionCode.Dashboard:
                    return 1;
                case UserOptionCode.DctMedical:
                    return 2;
                case UserOptionCode.DctNursing:
                    return 3;
                case UserOptionCode.DctAlliedHealth:
                    return 4;
                default:
                    return 99;
            }
        }
    }
}