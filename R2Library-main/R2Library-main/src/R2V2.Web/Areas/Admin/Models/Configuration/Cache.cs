#region

using System.Collections.Generic;
using R2V2.Web.Infrastructure.Storages;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Configuration
{
    public class Cache : AdminBaseModel
    {
        public IList<ApplicationStorageItem> Items { get; set; }
    }
}