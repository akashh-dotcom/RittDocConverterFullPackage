#region

using System.ServiceModel;

#endregion

#pragma warning disable 1591 //disables XML documentation warnings

namespace Sushi.Core
{
    /// <summary>
    ///     Provides an interface for services that process SUSHI requests for COUNTER data.
    /// </summary>
    [ServiceContractAttribute(Namespace = "SushiService", ConfigurationName = "Sushi.Core.ISushiService")]
    public interface ISushiService
    {
        [OperationContractAttribute(Action = "SushiService:GetReportIn", ReplyAction = "SushiService:GetReportIn")]
        [XmlSerializerFormatAttribute]
        [ServiceKnownTypeAttribute(typeof(Organization))]
        [ServiceKnownTypeAttribute(typeof(Activity))]
        [ServiceKnownTypeAttribute(typeof(ReportResponse))]
        GetReportResponse GetReport(GetReportRequest request);
    }
}