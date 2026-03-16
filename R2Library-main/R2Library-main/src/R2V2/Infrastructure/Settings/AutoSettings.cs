#region

using System.Collections.Generic;

#endregion

namespace R2V2.Infrastructure.Settings
{
    public abstract class AutoSettings : IAutoSettings
    {
        public List<string> MissingSettings { get; set; }
    }
}