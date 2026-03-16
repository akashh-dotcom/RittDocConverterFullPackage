#region

using System;
using R2V2.Infrastructure.DependencyInjection;

#endregion

namespace R2V2.Core.Institution
{
    [Serializable]
    [DoNotRegisterWithContainer]
    public class InstitutionAccountStatus : IInstitutionAccountStatus
    {
        public static readonly InstitutionAccountStatus All = new InstitutionAccountStatus
            { Id = AccountStatus.All, Description = "All" };

        public static readonly InstitutionAccountStatus Active = new InstitutionAccountStatus
            { Id = AccountStatus.Active, Description = "Active" };

        public static readonly InstitutionAccountStatus Trial = new InstitutionAccountStatus
            { Id = AccountStatus.Trial, Description = "Trial" };

        public static readonly InstitutionAccountStatus TrialExpired = new InstitutionAccountStatus
            { Id = AccountStatus.Trial, Description = "Trial Expired" };

        public static readonly InstitutionAccountStatus Disabled = new InstitutionAccountStatus
            { Id = AccountStatus.Disabled, Description = "Disabled" };

        public static readonly InstitutionAccountStatus PdaOnly = new InstitutionAccountStatus
            { Id = AccountStatus.PdaOnly, Description = "PDA" };

        private InstitutionAccountStatus()
        {
        }

        public AccountStatus Id { get; protected set; }
        public string Description { get; protected set; }
    }

    [Serializable]
    public static class AccountStatusExtensions
    {
        public static InstitutionAccountStatus ToInstitutionAccountStatus(this AccountStatus accountStatus)
        {
            switch (accountStatus)
            {
                case AccountStatus.Active:
                    return InstitutionAccountStatus.Active;

                case AccountStatus.Trial:
                    return InstitutionAccountStatus.Trial;

                case AccountStatus.TrialExpired:
                    return InstitutionAccountStatus.TrialExpired;

                case AccountStatus.Disabled:
                    return InstitutionAccountStatus.Disabled;

                case AccountStatus.PdaOnly:
                    return InstitutionAccountStatus.PdaOnly;

                case AccountStatus.All:
                default:
                    throw new ArgumentOutOfRangeException("accountStatus");
            }
        }
    }
}