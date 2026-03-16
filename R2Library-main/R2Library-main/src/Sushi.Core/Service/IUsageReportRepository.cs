namespace Sushi.Core
{
    /// <summary>
    ///     Defines an interface for objects that retrieve Counter report data
    ///     given a customer and a usage date range.
    /// </summary>
    /// <remarks>
    ///     Developers wishing to implement their own Sushi service should implement
    ///     this interface in their Data Access Layer.  This interface's goal should
    ///     be to execute a query against the appropriate usage repository to retrieve
    ///     the desired usage statistics, formatting those statistic into the Counter
    ///     Reports object graph.
    /// </remarks>
    public interface IUsageReportRepository
    {
        /// <summary>
        ///     Gets Counter report data given a customer and a usage date range.
        /// </summary>
        /// <param name="request">The request object from the webservice.</param>
        /// <returns>Counter report data for the given customer and date range.</returns>
        Report[] GetUsageReports(ReportRequest request);
    }
}