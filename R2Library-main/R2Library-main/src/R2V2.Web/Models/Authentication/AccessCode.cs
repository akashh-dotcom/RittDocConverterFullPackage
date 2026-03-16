namespace R2V2.Web.Models.Authentication
{
    public enum AccessCode
    {
        Allowed = 0,
        Unauthenticated = 1,
        Unauthorized = 2,
        UnauthorizedAthens = 3,
        UnknownParameters = 4
    }

    public static class AccessCodeExtensions
    {
        public static string ToDescription(this AccessCode? code)
        {
            switch (code)
            {
                case AccessCode.Unauthenticated:
                    return "Administrator access is required. Please login in the upper right corner to proceed.";
                case AccessCode.UnauthorizedAthens:
                    return "Your Athens credentials could not be verified.";
                case AccessCode.UnknownParameters:
                    return
                        "Some of the parameters in the URL are not formatted correctly. Please click the back button, refresh the page, and try the link again.";
                default:
                case AccessCode.Unauthorized:
                    return "You are not authorized to access this part of the system.";
            }
        }

        public static string ToLower(this AccessCode code)
        {
            switch (code)
            {
                case AccessCode.Allowed:
                    return "allowed";
                case AccessCode.Unauthenticated:
                    return "unauthenticated";
                case AccessCode.Unauthorized:
                    return "unauthorized";
                case AccessCode.UnauthorizedAthens:
                    return "unauthorizedathens";
                case AccessCode.UnknownParameters:
                    return "unknownparameters";
            }

            return null;
        }
    }
}