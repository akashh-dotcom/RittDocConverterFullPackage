#region

using System.Collections.Generic;

#endregion

namespace R2V2.Web.Models
{
    public interface IPageable
    {
        IEnumerable<PageLink> PageLinks { get; set; }
    }
}