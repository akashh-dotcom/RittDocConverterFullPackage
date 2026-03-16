#region

using System;
using System.Collections.Generic;
using R2V2.Core.MyR2;
using R2V2.Core.SuperType;
using R2V2.Core.Territory;

#endregion

namespace R2V2.Core.Authentication
{
    public interface IUser : ISoftDeletable, IDebugInfo
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        string Email { get; set; }
        Role Role { get; set; }
        int? InstitutionId { get; set; }
        Department Department { get; set; }
        DateTime? ExpirationDate { get; set; }
        string AthensTargetedId { get; set; }

        DateTime? ExpertReviewerRequestDate { get; set; }

        int Id { get; set; }

        DateTime? LastSession { get; set; }
        DateTime? ConcurrentTurnawayAlert { get; set; }

        DateTime? LastPasswordChange { get; set; }

        short? EnablePromotion { get; set; }
        short EnablePublisherAdd { get; set; }
        IEnumerable<UserTerritory> UserTerritories { get; set; }
        bool IsLocked { get; set; }
        IList<UserOptionValue> OptionValues { get; set; }

        IList<UserSubscription> Subscriptions { get; set; }
        void AddUserContentFolder(UserContentFolder userContentFolder);

        bool IsRittenhouseAdmin();
        bool IsInstitutionAdmin();
        bool IsPublisherUser();
        bool IsSalesAssociate();
        bool IsExpertReviewer();
    }
}