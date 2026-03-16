#region

using System;

#endregion

namespace R2V2.Core.Authentication
{
    public class GuestUser
    {
        public virtual int Id { get; set; }

        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }
        public virtual string Email { get; set; }
        public virtual bool RecordStatus { get; set; }
        public virtual Role Role { get; set; }

        public virtual string CreatedBy { get; set; }
        public virtual DateTime CreationDate { get; set; }

        public virtual int? InstitutionId { get; set; }

        public virtual string PasswordHash { get; set; }
        public virtual string PasswordSalt { get; set; }
        public virtual DateTime? LastPasswordChange { get; set; }
        public virtual int LoginAttempts { get; set; }

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

        public virtual bool ReceiveForthComingPurchase
        {
            get => false;
            set { }
        }
    }
}