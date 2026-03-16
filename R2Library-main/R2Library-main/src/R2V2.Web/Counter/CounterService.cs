#region

using R2V2.Infrastructure.DependencyInjection;
using R2V2.Core;
using R2V2.Core.Institution;
using R2V2.Web.Areas.Admin.Models.Report;

#endregion

namespace R2V2.Web.Counter
{
    public class CounterService
    {
        public CounterReportService CounterReportService = ServiceLocator.Current.GetInstance<CounterReportService>();
        public InstitutionService InstitutionService = ServiceLocator.Current.GetInstance<InstitutionService>();
        public UserService UserService = ServiceLocator.Current.GetInstance<UserService>();
    }
}