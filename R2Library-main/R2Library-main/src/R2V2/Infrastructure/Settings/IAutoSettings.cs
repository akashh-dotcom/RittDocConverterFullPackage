#region

using System.Collections.Generic;

#endregion

namespace R2V2.Infrastructure.Settings
{
    public interface IAutoSettings
    {
        List<string> MissingSettings { get; }
    }
}