#region

using System.Collections.Generic;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Cart
{
    public class SavedCarts : AdminBaseModel
    {
        public SavedCarts()
        {
        }

        public SavedCarts(IAdminInstitution institution) : base(institution)
        {
        }

        public IEnumerable<CachedCart> CachedCarts { get; set; }
    }
}