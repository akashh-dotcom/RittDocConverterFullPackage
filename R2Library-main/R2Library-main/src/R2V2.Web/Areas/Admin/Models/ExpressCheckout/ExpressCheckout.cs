#region

using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.ExpressCheckout
{
    public class ExpressCheckout : AdminBaseModel
    {
        public ExpressCheckout()
        {
        }

        public ExpressCheckout(IAdminInstitution institution)
            : base(institution)
        {
        }

        public CollectionManagementQuery CollectionManagementQuery { get; set; }
        public string Isbns { get; set; }
    }
}