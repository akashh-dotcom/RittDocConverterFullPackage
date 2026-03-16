#region

using System;
using System.Collections.Generic;
using R2V2.Core.Authentication;

#endregion

namespace R2V2.Core.Admin
{
    public interface IAdminAlert
    {
        int Id { get; set; }
        string CreatedBy { get; set; }
        string UpdatedBy { get; set; }
        DateTime CreationDate { get; set; }
        DateTime? LastUpdated { get; set; }
        bool RecordStatus { get; set; }
        bool DisplayOnce { get; set; }
        string Title { get; set; }
        string Text { get; set; }
        DateTime? StartDate { get; set; }
        DateTime? EndDate { get; set; }
        string AlertName { get; set; }
        Role Role { get; set; }
        int RoleId { get; set; }
        AlertLayout Layout { get; set; }
        IEnumerable<AlertImage> AlertImages { get; set; }
        int? ResourceId { get; set; }
        bool AllowPurchase { get; set; }
        bool AllowPDA { get; set; }
    }
}