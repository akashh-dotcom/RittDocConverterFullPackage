#region

using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.ReserveShelfManagement
{
    public class ReserveShelfUrlManagement : AdminBaseModel
    {
        public ReserveShelfUrlManagement()
        {
        }

        public ReserveShelfUrlManagement(AdminInstitution institution, ReserveShelfUrl reserveShelfUrl)
            : base(institution)
        {
            ReserveShelfUrl = reserveShelfUrl;
        }

        public ReserveShelfUrl ReserveShelfUrl { get; set; }
    }
}