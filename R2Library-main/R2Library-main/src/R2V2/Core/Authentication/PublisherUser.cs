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
    public class PublisherUser : AuditableEntity, IUserWithFolders
    {
        public virtual Publisher.Publisher Publisher { get; set; }

        public virtual bool ReceiveLockoutInfo
        {
            get => false;
            set { }
        }

        public virtual bool ReceiveNewResourceInfo
        {
            get => false;
            set { }
        }

        public virtual bool ReceiveNewSearchResource
        {
            get => false;
            set { }
        }

        public virtual Institution.Institution Institution { get; set; }

        public virtual bool ReceiveNewEditionInfo
        {
            get => false;
            set { }
        }

        public virtual bool ReceiveCartRemind
        {
            get => false;
            set { }
        }

        public virtual bool ReceiveForthcomingPurchase
        {
            get => false;
            set { }
        }

        public virtual bool ReceivePdaAddToCart
        {
            get => false;
            set { }
        }

        public virtual bool ReceivePdaReport
        {
            get => false;
            set { }
        }

        public virtual bool ReceiveArchivedAlert
        {
            get => false;
            set { }
        }

        public virtual bool ReceiveLibrarianAlert
        {
            get => false;
            set { }
        }

        public virtual bool ReceiveAnnualMaintenanceFee
        {
            get => false;
            set { }
        }

        public virtual bool ReceiveDashboardEmail
        {
            get => false;
            set { }
        }

        public virtual bool ReceiveExpertReviewerRequests { get; set; }
        public virtual bool ReceiveExpertReviewerRecommendations { get; set; }

        public virtual bool ReceiveDctMedicalUpdate
        {
            get => false;
            set { }
        }

        public virtual bool ReceiveDctNursingUpdate
        {
            get => false;
            set { }
        }

        public virtual bool ReceiveDctAlliedHealthUpdate
        {
            get => false;
            set { }
        }

        public virtual bool RecordStatus { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }
        public virtual Role Role { get; set; }


        public virtual void AddUserContentFolder(UserContentFolder userContentFolder)
        {
        }

        public virtual IEnumerable<UserBookmarkFolder> UserBookmarkFolders => null;
        public virtual IEnumerable<UserCourseLinksFolder> UserCourseLinksFolders => null;
        public virtual IEnumerable<UserImagesFolder> UserImagesFolders => null;
        public virtual IEnumerable<UserReferencesFolder> UserReferencesFolders => null;

        public virtual string FirstName
        {
            get => null;
            set { }
        }

        public virtual string LastName
        {
            get => null;
            set { }
        }

        public virtual string Email
        {
            get => null;
            set { }
        }

        public virtual int? InstitutionId
        {
            get => -1;
            set { }
        }

        public virtual Department Department
        {
            get => null;
            set { }
        }

        public virtual DateTime? ExpirationDate
        {
            get => null;
            set { }
        }

        public virtual string AthensTargetedId
        {
            get => null;
            set { }
        }

        public virtual bool IsLocked
        {
            get => false;
            set { }
        }

        public virtual IList<UserOptionValue> OptionValues
        {
            get => null;
            set { }
        }

        public virtual DateTime? LastSession
        {
            get => null;
            set { }
        }

        public virtual DateTime? ConcurrentTurnawayAlert
        {
            get => null;
            set { }
        }


        public virtual DateTime? LastPasswordChange
        {
            get => null;
            set { }
        }

        public virtual short? EnablePromotion
        {
            get => 0;
            set { }
        }

        public virtual short EnablePublisherAdd
        {
            get => 0;
            set { }
        }

        public virtual DateTime? ExpertReviewerRequestDate { get; set; }
        public virtual IList<UserSubscription> Subscriptions { get; set; }

        public virtual IEnumerable<UserTerritory> UserTerritories
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

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

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("PublisherUser = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", FirstName: {0}", FirstName);
            sb.AppendFormat(", LastName: {0}", LastName);
            sb.AppendFormat(", UserName: {0}", UserName);
            sb.AppendFormat(", Email: {0}", Email);
            sb.AppendFormat(" Role: {0}", Role == null ? "null" : Role.ToString());
            sb.AppendFormat(" Institution: {0}", Institution == null ? "null" : Institution.ToString());
            sb.Append("]");
            return sb.ToString();
        }

        public virtual void AddUserOptionValue(UserOptionValue userOptionValue)
        {
            throw new NotImplementedException();
        }
    }
}