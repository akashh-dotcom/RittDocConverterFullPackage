#region

using System.Collections.Generic;

#endregion

namespace R2V2.Web.Models
{
    public class LogTesterViewModel
    {
        public int DelayTime { get; set; }

        public List<string> Messages { get; set; } = new List<string>();
    }
}