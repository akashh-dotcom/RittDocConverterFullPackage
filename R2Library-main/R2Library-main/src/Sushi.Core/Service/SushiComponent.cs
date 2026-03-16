#region

using System;

#endregion

namespace Sushi.Core.Service
{
    /// <summary>
    ///     Returns report results from UsageReportRepository while checking AuthorizationAuthority for access
    /// </summary>
    public class SushiComponent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SushiComponent" /> class.
        /// </summary>
        /// <param name="reportRepository">
        ///     The <see cref="IUsageReportRepository" /> instance to use for Counter usage data retrieval.
        /// </param>
        /// <param name="authorizationAuthority">
        ///     The <see cref="IAuthorizationAuthority" /> instance to use for requestor / customer authorization.
        /// </param>
        public SushiComponent(IUsageReportRepository reportRepository, IAuthorizationAuthority authorizationAuthority)
        {
            ReportRepository = reportRepository;
            AuthorizationAuthority = authorizationAuthority;
        }

        /// <summary>
        ///     Gets the <see cref="IUsageReportRepository" /> instance that was injected when this instance was created.
        /// </summary>
        public IUsageReportRepository ReportRepository { get; }

        /// <summary>
        ///     Gets the <see cref="IAuthorizationAuthority" /> instance that was injected when this instance was created.
        /// </summary>
        public IAuthorizationAuthority AuthorizationAuthority { get; }

        /// <summary>
        ///     Retrieves an instance of the <see cref="Reports" /> class containing the requested usage statistics.
        /// </summary>
        /// <param name="request">The request object from the webservice.</param>
        /// <returns>An instance of the <see cref="Reports" /> class containing the requested usage statistics.</returns>
        public Report[] GetSushiReports(ReportRequest request)
        {
            var authorized = AuthorizationAuthority.IsRequestorAuthorized(request.Requestor, request.CustomerReference);

            if (!authorized)
            {
                throw new InvalidOperationException("Requestor is not authorized to view usage for this customer.");
            }

            return ReportRepository.GetUsageReports(request);
        }
    }
}