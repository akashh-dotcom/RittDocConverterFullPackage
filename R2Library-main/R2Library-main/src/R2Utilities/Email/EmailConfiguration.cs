#region

using System.Collections.Generic;

#endregion

namespace R2Utilities.Email
{
    public class EmailConfiguration
    {
        public bool Send { get; set; } = true;
        public string Type { get; set; }
        public List<string> ToAddresses { get; private set; } = new List<string> { "r2errors@technotects.net" };
        public List<string> CcAddresses { get; private set; } = new List<string> { "r2errors@technotects.net" };
        public List<string> BccAddresses { get; private set; } = new List<string> { "r2errors@technotects.net" };

        // set the email addresses to error emails, if a task is sending to error emails then there is a config issue
    }
}