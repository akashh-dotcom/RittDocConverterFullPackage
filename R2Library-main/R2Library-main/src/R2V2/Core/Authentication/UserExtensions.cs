namespace R2V2.Core.Authentication
{
    public static class UserExtensions
    {
        public static string ToFullName(this IUser user)
        {
            return ToFullName(user, false);
        }

        public static string ToFullName(this IUser user, bool lastNameFirst)
        {
            if (string.IsNullOrWhiteSpace(user.LastName))
            {
                return string.IsNullOrWhiteSpace(user.FirstName) ? string.Empty : user.FirstName;
            }

            if (lastNameFirst)
            {
                return string.IsNullOrWhiteSpace(user.FirstName) ? user.LastName : $"{user.LastName}, {user.FirstName}";
            }

            return string.IsNullOrWhiteSpace(user.FirstName) ? user.LastName : $"{user.FirstName} {user.LastName}";
        }
    }
}