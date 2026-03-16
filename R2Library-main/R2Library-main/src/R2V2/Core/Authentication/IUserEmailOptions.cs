#region

using System.Collections.Generic;

#endregion

namespace R2V2.Core.Authentication
{
    public interface IUserEmailOptions
    {
        int UserId { get; set; }
        List<UserEmailOption> EmailOptions { get; set; }

        string SubscribedOptions { get; set; }
        string UnSubscribedOptions { get; set; }

        string ActionName { get; set; }
        string ControllerName { get; set; }

        bool IsSelf { get; set; }

        IEnumerable<UserEmailOption> OtherOptions();
        IEnumerable<UserEmailOption> WeeklyOptions();
        IEnumerable<UserEmailOption> MonthlyOptions();
    }
}