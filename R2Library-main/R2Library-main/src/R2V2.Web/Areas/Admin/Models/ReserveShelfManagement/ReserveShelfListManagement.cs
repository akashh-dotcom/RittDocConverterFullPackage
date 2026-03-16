#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.ReserveShelfManagement
{
    public class ReserveShelfListManagement : AdminBaseModel
    {
        public ReserveShelfListManagement()
        {
        }

        public ReserveShelfListManagement(IAdminInstitution institution, List<ReserveShelfList> reserveShelfLists)
            : base(institution)
        {
            if (reserveShelfLists.Any())
            {
                ReserveShelfLists = reserveShelfLists.OrderBy(x => x.Name).ToList();
            }
        }

        public ReserveShelfListManagement(IAdminInstitution institution, ReserveShelfList editReserveShelf)
            : base(institution)
        {
            EditReserveShelf = editReserveShelf;
        }

        public ReserveShelfListManagement(IAdminInstitution institution, ReserveShelfList editReserveShelf,
            ReserveShelfUrl editReserveShelfUrl)
            : base(institution)
        {
            EditReserveShelf = editReserveShelf;
            EditReserveShelfUrl = editReserveShelfUrl;
        }

        public List<ReserveShelfList> ReserveShelfLists { get; set; }
        public ReserveShelfList EditReserveShelf { get; set; }

        public ReserveShelfUrl EditReserveShelfUrl { get; set; }
    }
}