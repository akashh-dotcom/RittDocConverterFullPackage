#region

using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class CounterIndexDetail : AdminBaseModel
    {
        public CounterIndexDetail(IAdminInstitution institution)
            : base(institution)
        {
            DefaultQueries = new CounterReportDefaultQueries(institution);
        }

        public CounterReportDefaultQueries DefaultQueries { get; }
    }
}