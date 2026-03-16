#region

using System.Collections.Generic;

#endregion

namespace R2V2.Web.Models.Shared
{
    public class SortGroup
    {
        public Dictionary<string, Filter> Filters { get; set; } = new Dictionary<string, Filter>();

        public string Name { get; set; }
        public string Code { get; set; }
    }
}