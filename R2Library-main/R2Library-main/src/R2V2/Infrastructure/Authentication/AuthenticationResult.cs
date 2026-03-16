#region

using R2V2.Core.Authentication;
using R2V2.Infrastructure.DependencyInjection;

#endregion

namespace R2V2.Infrastructure.Authentication
{
    [DoNotRegisterWithContainer]
    public class AuthenticationResult
    {
        private AuthenticationResult()
        {
        }

        public bool WasSuccessful { get; private set; }
        public bool WasBlocked { get; private set; }
        public bool WasAutoLocked { get; private set; }
        public bool WasAttemptLocked { get; private set; }

        public string AthensTargetedId { get; set; }

        public AuthenticatedInstitution AuthenticatedInstitution { get; private set; }
        public User LockedUser { get; set; }

        public static AuthenticationResult Successful(AuthenticatedInstitution authenticatedInstitution)
        {
            return new AuthenticationResult
            {
                WasSuccessful = true,
                AuthenticatedInstitution = authenticatedInstitution,
                WasBlocked = false,
                WasAutoLocked = false,
                WasAttemptLocked = false
            };
        }

        public static AuthenticationResult Successful(AuthenticatedInstitution authenticatedInstitution,
            string athensTargetedId)
        {
            return new AuthenticationResult
            {
                WasSuccessful = true,
                AuthenticatedInstitution = authenticatedInstitution,
                WasBlocked = false,
                WasAutoLocked = false,
                WasAttemptLocked = false,
                AthensTargetedId = athensTargetedId
            };
        }

        public static AuthenticationResult Failed()
        {
            return new AuthenticationResult
            {
                WasSuccessful = false,
                WasBlocked = false,
                WasAutoLocked = false,
                WasAttemptLocked = false
            };
        }

        public static AuthenticationResult Blocked()
        {
            return new AuthenticationResult
            {
                WasSuccessful = false,
                WasBlocked = true,
                WasAutoLocked = false,
                WasAttemptLocked = false
            };
        }

        public static AuthenticationResult AutoLocked(User lockedUser)
        {
            return new AuthenticationResult
            {
                LockedUser = lockedUser,
                WasSuccessful = false,
                WasBlocked = false,
                WasAutoLocked = true,
                WasAttemptLocked = false
            };
        }

        public static AuthenticationResult AttemptLocked()
        {
            return new AuthenticationResult
            {
                WasSuccessful = false,
                WasBlocked = false,
                WasAutoLocked = false,
                WasAttemptLocked = true
            };
        }
    }
}