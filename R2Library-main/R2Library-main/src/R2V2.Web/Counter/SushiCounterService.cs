#region

using System;
using System.Linq;
using Sushi.Core;
using Sushi.Core.Service;
using Exception = System.Exception;

#endregion


namespace R2V2.Web.Counter
{
    public class SushiCounterService : ISushiService
    {
        private readonly CounterService _counterService = new CounterService();

        public GetReportResponse GetReport(GetReportRequest request)
        {
            var businessLogic = new SushiComponent(
                new UsageReportRepository(_counterService.InstitutionService, _counterService.CounterReportService),
                new AuthorizationAuthority(_counterService.InstitutionService, _counterService.UserService));

            var response = new GetReportResponse
            {
                ReportResponse = new CounterReportResponse
                {
                    Created = DateTime.Now,
                    CreatedSpecified = true,
                    CustomerReference = request.ReportRequest.CustomerReference,
                    ID = request.ReportRequest.ID,
                    ReportDefinition = request.ReportRequest.ReportDefinition,
                    Requestor = request.ReportRequest.Requestor
                }
            };
            try
            {
                response.ReportResponse.Report = businessLogic.GetSushiReports(request.ReportRequest);
            }
            catch (Exception ex)
            {
                response.ReportResponse.Exception = ex.ToSushiExceptions().ToArray();
            }

            return response;
        }
    }
}