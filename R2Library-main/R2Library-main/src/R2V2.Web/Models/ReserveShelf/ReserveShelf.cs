#region

using System.Collections.Generic;

#endregion

namespace R2V2.Web.Models.ReserveShelf
{
    public class ReserveShelf
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public IEnumerable<IResourceSummary> Resources { get; set; }
    }
}