#region

using System;
using R2V2.Core.Institution;
using R2V2.Infrastructure.Authentication;

#endregion

namespace R2V2.Web.Models
{
    [Serializable]
    public class AuthenticationInfo
    {
        public AuthenticationInfo(string displaName, bool isInstitutionUser, int userId, bool isNotGuest,
            bool isAuthenticated, AuthenticatedInstitution institution)
        {
            UserId = userId;
            DisplayName = string.IsNullOrWhiteSpace(displaName) ? "" : displaName.Trim();
            IsInstitutionUser = isInstitutionUser;
            IsNotGuest = isNotGuest;
            IsAuthenticated = isAuthenticated;

            if (institution == null)
            {
                return;
            }

            HomePage = institution.HomePage;
            CanIpAuthCreateUsers = institution.AccessType.Equals(AccessType.IpValidationOpt);
            MessageDisplayName = string.IsNullOrWhiteSpace(institution.DisplayName)
                ? institution.Name
                : institution.DisplayName;
            InstitutionMessage = institution.BrandingMessage;
            IsInstitutionNameForDisplay = institution.IsInstitutionNameForDisplay;
            IsAthensAuthenticated = institution.AuthenticationMethod == AuthenticationMethods.AthensInstitution ||
                                    institution.AuthenticationMethod == AuthenticationMethods.AthensUser;
        }

        public int UserId { get; private set; }
        public string DisplayName { get; private set; }
        public string DisplayDate => DateTime.Now.ToLongDateString();
        public bool IsInstitutionUser { get; private set; }
        public bool IsNotGuest { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public string MessageDisplayName { get; private set; }
        public string InstitutionMessage { get; private set; }
        public HomePage HomePage { get; private set; }
        public bool CanIpAuthCreateUsers { get; private set; }

        public bool IsInstitutionNameForDisplay { get; set; }
        public bool IsAthensAuthenticated { get; private set; }
    }
}