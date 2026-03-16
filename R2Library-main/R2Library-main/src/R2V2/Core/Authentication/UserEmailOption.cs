namespace R2V2.Core.Authentication
{
    public class UserEmailOption
    {
        public UserEmailOption()
        {
        }

        public UserEmailOption(UserOptionValue optionValue)
        {
            Id = optionValue.Id;
            Code = optionValue.Option.Code;
            Description = optionValue.Option.Description;
            Value = optionValue.Value == "1";
            UserEmailGroupCode = GetGroupCode(optionValue.Option.Code);
            ListPosition = optionValue.ToPositition();
        }

        public int Id { get; set; }
        public UserOptionCode Code { get; set; }
        public string Description { get; set; }
        public bool Value { get; set; }

        public UserEmailGroupCode UserEmailGroupCode { get; set; }
        public int ListPosition { get; set; }


        private static UserEmailGroupCode GetGroupCode(UserOptionCode optionCode)
        {
            switch (optionCode)
            {
                case UserOptionCode.NewResource:
                case UserOptionCode.NewEdition:
                case UserOptionCode.PdaReport:
                case UserOptionCode.ArchivedAlert:
                case UserOptionCode.ForthcomingPurchase:
                    return UserEmailGroupCode.Weekly;
                case UserOptionCode.DctMedical:
                case UserOptionCode.DctNursing:
                case UserOptionCode.DctAlliedHealth:
                case UserOptionCode.Dashboard:
                    return UserEmailGroupCode.Monthly;
                case UserOptionCode.AccessDenied:
                case UserOptionCode.PdaAddToCart:
                case UserOptionCode.LibrarianAlert:
                case UserOptionCode.ExpertReviewRecommend:
                case UserOptionCode.ExpertReviewUserRequest:
                case UserOptionCode.AnnualMaintenanceFee:
                case UserOptionCode.CartRemind:
                default:
                    return UserEmailGroupCode.Other;
            }
        }
    }
}