#region

using System;
using R2V2.Extensions;

#endregion

namespace R2V2.Web.Models
{
    [Serializable]
    public class Footer
    {
        public string CopyrightText { get; private set; } =
            "2003 - {0} Rittenhouse Digital, LLC, Inc. All rights reserved".Args(DateTime.Now.Year);
    }
}