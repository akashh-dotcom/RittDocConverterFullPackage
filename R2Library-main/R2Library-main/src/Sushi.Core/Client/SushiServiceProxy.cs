#region

using System.ServiceModel;

#endregion

namespace Sushi.Core
{
    /// <summary>
    ///     Provides a proxy to a hosted instance of the <see cref="ISushiService" /> interface.
    /// </summary>
    public class SushiServiceProxy : ClientBase<ISushiService>, ISushiService
    {
        /// <summary>
        ///     Executes a proxied request to the <see cref="ISushiService" /> method.
        /// </summary>
        /// <param name="request">The report request.</param>
        /// <returns>The deserialized report response.</returns>
        public GetReportResponse GetReport(GetReportRequest request)
        {
            return Channel.GetReport(request);
        }
    }
}