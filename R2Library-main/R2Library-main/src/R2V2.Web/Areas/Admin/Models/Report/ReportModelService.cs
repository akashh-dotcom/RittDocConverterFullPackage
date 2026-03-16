#region

using R2V2.Core.Publisher;
using R2V2.Core.Resource;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class ReportModelService
    {
        public ReportModelService(ILog<ReportModelService> log
            , IPracticeAreaService practiceAreaService
            , IResourceService resourceService
            , IPublisherService publisherService
        )
        {
        }
    }
}