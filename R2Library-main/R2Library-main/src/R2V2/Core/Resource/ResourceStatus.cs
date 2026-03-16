#region

using System;

#endregion

namespace R2V2.Core.Resource
{
    public enum ResourceStatus
    {
        All = 0,
        Active = 6,
        Archived = 7,
        Forthcoming = 8,
        Inactive = 72,
        QANotApproved = 100,
        NotPromoted = 101
    }

    [Flags]
    public enum Include
    {
        Active = 1,
        Archive = 2,
        ActiveAndArchive = 3
    }
}