#region

using System;
using System.Collections.Generic;
using System.Text;
using R2V2.Core.MyR2;
using R2V2.Core.Territory;

#endregion

namespace R2V2.Core.Authentication
{
    [Serializable]
    public class CachedUser : IUser
    {
        private readonly IList<IMyR2Folder> _userBookmarkFolders;
        private readonly IList<IMyR2Folder> _userCourseLinksFolders;
        private readonly IList<IMyR2Folder> _userImagesFolders;
        private readonly IList<IMyR2Folder> _userReferencesFolders;

        public CachedUser(IUserWithFolders user)
        {
            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            UserName = user.UserName;
            Password = user.Password;
            Email = user.Email;
            RecordStatus = user.RecordStatus;
            Role = user.Role;
            InstitutionId = user.InstitutionId.GetValueOrDefault();
            Department = user.Department;
            ExpirationDate = user.ExpirationDate;
            AthensTargetedId = user.AthensTargetedId;

            LastSession = user.LastSession;
            ConcurrentTurnawayAlert = user.ConcurrentTurnawayAlert;
            EnablePromotion = user.EnablePromotion;
            EnablePublisherAdd = user.EnablePublisherAdd;

            ExpertReviewerRequestDate = user.ExpertReviewerRequestDate;

            OptionValues = user.OptionValues;
            Subscriptions = user.Subscriptions;

            _userBookmarkFolders = new List<IMyR2Folder>();
            if (user.UserBookmarkFolders != null)
            {
                foreach (var folder in user.UserBookmarkFolders)
                {
                    _userBookmarkFolders.Add(new MyR2Folder(folder));
                }
            }

            _userCourseLinksFolders = new List<IMyR2Folder>();
            if (user.UserCourseLinksFolders != null)
            {
                foreach (var folder in user.UserCourseLinksFolders)
                {
                    _userCourseLinksFolders.Add(new MyR2Folder(folder));
                }
            }

            _userImagesFolders = new List<IMyR2Folder>();
            if (user.UserImagesFolders != null)
            {
                foreach (var folder in user.UserImagesFolders)
                {
                    _userImagesFolders.Add(new MyR2Folder(folder));
                }
            }

            _userReferencesFolders = new List<IMyR2Folder>();
            if (user.UserReferencesFolders != null)
            {
                foreach (var folder in user.UserReferencesFolders)
                {
                    _userReferencesFolders.Add(new MyR2Folder(folder));
                }
            }
        }

        public IEnumerable<IMyR2Folder> UserBookmarkFolders => _userBookmarkFolders;

        public IEnumerable<IMyR2Folder> UserCourseLinksFolders => _userCourseLinksFolders;

        public IEnumerable<IMyR2Folder> UserImagesFolders => _userImagesFolders;

        public IEnumerable<IMyR2Folder> UserReferencesFolders => _userReferencesFolders;

        public DateTime? CreationDate { get; set; }

        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool RecordStatus { get; set; }
        public Role Role { get; set; }
        public int? InstitutionId { get; set; }
        public Department Department { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string AthensTargetedId { get; set; }

        public DateTime? LastSession { get; set; }
        public DateTime? ConcurrentTurnawayAlert { get; set; }

        public DateTime? LastPasswordChange { get; set; }

        public short? EnablePromotion { get; set; }
        public short EnablePublisherAdd { get; set; }


        public bool IsLocked { get; set; }

        public DateTime? ExpertReviewerRequestDate { get; set; }

        public bool IsRittenhouseAdmin()
        {
            return Role.Code == RoleCode.RITADMIN;
        }

        public bool IsInstitutionAdmin()
        {
            return Role.Code == RoleCode.INSTADMIN;
        }

        public bool IsPublisherUser()
        {
            return Role.Code == RoleCode.PUBUSER;
        }

        public bool IsSalesAssociate()
        {
            return Role.Code == RoleCode.SALESASSOC;
        }

        public bool IsExpertReviewer()
        {
            return Role.Code == RoleCode.ExpertReviewer;
        }

        public IEnumerable<UserTerritory> UserTerritories
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public IList<UserOptionValue> OptionValues { get; set; }
        public IList<UserSubscription> Subscriptions { get; set; }

        public void AddUserContentFolder(UserContentFolder userContentFolder)
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

            userContentFolder.UserId = Id;
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder("CachedUser = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", FirstName: {0}", FirstName);
            sb.AppendFormat(", LastName: {0}", LastName);
            sb.AppendFormat(", UserName: {0}", UserName);
            sb.AppendFormat(", Email: {0}", Email);
            sb.AppendFormat(", Role: {0}", Role == null ? "null" : Role.ToString());
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", _userBookmarkFolders.Count: {0}", _userBookmarkFolders.Count);
            sb.AppendFormat(", _userCourseLinksFolders.Count: {0}", _userCourseLinksFolders.Count);
            sb.AppendFormat(", _userImagesFolders.Count: {0}", _userImagesFolders.Count);
            sb.AppendFormat(", _userReferencesFolders.Count: {0}", _userReferencesFolders.Count);
            sb.Append("]");
            return sb.ToString();
        }
    }
}