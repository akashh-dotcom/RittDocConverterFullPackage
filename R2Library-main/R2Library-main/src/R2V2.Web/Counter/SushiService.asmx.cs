#region

using System.ComponentModel;
using System.Web.Script.Services;
using System.Web.Services;
using R2V2.Infrastructure.Logging;
using Sushi.Core;

#endregion

namespace R2V2.Web.Counter
{
    /// <summary>
    ///     Summary description for SushiService
    /// </summary>
    [WebService(Namespace = "http://r2library.com/counter/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [ScriptService]
    public class SushiService : WebService, ISushiService
    {
        private static readonly ILog<SushiService> Log = new Log<SushiService>();

        [WebMethod]
        public GetReportResponse GetReport(GetReportRequest request)
        {
            if (request == null)
            {
                return null;
            }

            Log.InfoFormat("GetReport Started for Customer: {0}", request.ReportRequest.CustomerReference.ID);


            var sushiCounterService = new SushiCounterService();
            return sushiCounterService.GetReport(request);
        }
    }
}