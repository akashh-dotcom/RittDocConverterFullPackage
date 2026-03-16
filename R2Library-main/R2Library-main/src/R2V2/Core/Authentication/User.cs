#region

using System;
using System.Collections.Generic;
using System.Text;
using R2V2.Core.MyR2;
using R2V2.Core.SuperType;
using R2V2.Core.Territory;

#endregion

namespace R2V2.Core.Authentication
{
    [Serializable]
    public class User : AuditableEntity, IUserWithFolders
    {
        private readonly IList<UserBookmarkFolder> _userBookmarkFolders = new List<UserBookmarkFolder>();
        private readonly IList<UserCourseLinksFolder> _userCourseLinksFolders = new List<UserCourseLinksFolder>();
        private readonly IList<UserImagesFolder> _userImagesFolders = new List<UserImagesFolder>();
        private readonly IList<UserReferencesFolder> _userReferencesFolders = new List<UserReferencesFolder>();

        private readonly IList<UserSavedSearchResultFolder> _userSavedSearchResultFolder =
            new List<UserSavedSearchResultFolder>();

        public virtual string PasswordHash { get; set; }
        public virtual string PasswordSalt { get; set; }
        public virtual Institution.Institution Institution { get; set; }
        public virtual string AthensUserName { get; set; }
        public virtual string AthensPersistentUid { get; set; }
        public virtual int LoginAttempts { get; set; }

        public virtual IEnumerable<UserSavedSearchResultFolder> UserSavedSearchResultFolders =>
            _userSavedSearchResultFolder;

        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }
        public virtual string Email { get; set; }
        public virtual bool RecordStatus { get; set; }
        public virtual Role Role { get; set; }
        public virtual int? InstitutionId { get; set; }
        public virtual Department Department { get; set; }
        public virtual DateTime? ExpirationDate { get; set; }
        public virtual string AthensTargetedId { get; set; }
        public virtual DateTime? LastSession { get; set; }
        public virtual DateTime? ConcurrentTurnawayAlert { get; set; }
        public virtual DateTime? LastPasswordChange { get; set; }
        public virtual short? EnablePromotion { get; set; }
        public virtual short EnablePublisherAdd { get; set; }
        public virtual DateTime? ExpertReviewerRequestDate { get; set; }
        public virtual IEnumerable<UserTerritory> UserTerritories { get; set; }
        public virtual IList<UserOptionValue> OptionValues { get; set; }

        public virtual IList<UserSubscription> Subscriptions { get; set; }

        public virtual void AddUserContentFolder(UserContentFolder userContentFolder)
        {
            var userBookmarkFolder = userContentFolder as UserBookmarkFolder;
            if (userBookmarkFolder != null)
            {
                _userBookmarkFolders.Add(userBookmarkFolder);
            }

            var userCourseLinksFolder = userContentFolder as UserCourseLinksFolder;
            if (userCourseLinksFolder != null)
            {
                _userCourseLinksFolders.Add(userCourseLinksFolder);
            }

            var userImagesFolder = userContentFolder as UserImagesFolder;
            if (userImagesFolder != null)
            {
                _userImagesFolders.Add(userImagesFolder);
            }

            var userReferencesFolder = userContentFolder as UserReferencesFolder;
            if (userReferencesFolder != null)
            {
                _userReferencesFolders.Add(userReferencesFolder);
            }

            var userSavedSearchResults = userContentFolder as UserSavedSearchResultFolder;
            if (userSavedSearchResults != null)
            {
                _userSavedSearchResultFolder.Add(userSavedSearchResults);
            }

            userContentFolder.UserId = Id;
        }

        public virtual IEnumerable<UserBookmarkFolder> UserBookmarkFolders => _userBookmarkFolders;

        public virtual IEnumerable<UserCourseLinksFolder> UserCourseLinksFolders => _userCourseLinksFolders;

        public virtual IEnumerable<UserImagesFolder> UserImagesFolders => _userImagesFolders;

        public virtual IEnumerable<UserReferencesFolder> UserReferencesFolders => _userReferencesFolders;

        public virtual bool IsRittenhouseAdmin()
        {
            return Role.Code == RoleCode.RITADMIN;
        }

        public virtual bool IsInstitutionAdmin()
        {
            return Role.Code == RoleCode.INSTADMIN;
        }

        public virtual bool IsPublisherUser()
        {
            return Role.Code == RoleCode.PUBUSER;
        }

        public virtual bool IsSalesAssociate()
        {
            return Role.Code == RoleCode.SALESASSOC;
        }

        public virtual bool IsExpertReviewer()
        {
            return Role.Code == RoleCode.ExpertReviewer;
        }

        //Can only set in R2V2.Web. Need to access ClientSettings. Need this here so it can be accessed in the core.
        public virtual bool IsLocked { get; set; }


        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("User = [");

            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", FirstName: {0}", FirstName);
            sb.AppendFormat(", LastName: {0}", LastName);
            sb.AppendFormat(", UserName: {0}", UserName);
            sb.AppendFormat(", Email: {0}", Email);
            sb.AppendFormat(", LastSession: {0}", LastSession);
            sb.AppendFormat(", EnablePromotion: {0}", EnablePromotion);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.AppendFormat(", Role: {0}", Role == null ? "null" : Role.ToDebugString());
            sb.AppendFormat(", Department: {0}", Department == null ? "null" : Department.ToDebugString());
            sb.AppendFormat(", Institution: {0}", Institution == null ? "null" : Institution.ToDebugString());
            sb.AppendLine().AppendFormat("\t, UserTerritories:");

            if (UserTerritories == null)
            {
                sb.Append(" null");
            }
            else
            {
                foreach (var userTerritory in UserTerritories)
                {
                    sb.AppendLine().AppendFormat("\t\t, {0}",
                        userTerritory == null ? "null" : userTerritory.ToDebugString());
                }
            }

            sb.AppendLine().Append("]");
            return sb.ToString();
        }

        public virtual bool IsSubscriptionUser()
        {
            return Role.Code == RoleCode.SUBUSER;
        }
    }
}