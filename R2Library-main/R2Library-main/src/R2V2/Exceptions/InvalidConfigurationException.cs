#region

using System;
using System.Collections.Generic;
using R2V2.Extensions;

#endregion

namespace R2V2.Exceptions
{
    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(IEnumerable<string> missingSettings)
            : base(missingSettings.ToDelimitedList("\n"))
        {
        }
    }
}