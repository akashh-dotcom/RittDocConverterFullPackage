#region

using System;

#endregion

namespace R2V2.Core.SuperType
{
    public interface IAuditable
    {
        string CreatedBy { get; set; }
        DateTime CreationDate { get; set; }
        string UpdatedBy { get; set; }
        DateTime? LastUpdated { get; set; }
    }
}