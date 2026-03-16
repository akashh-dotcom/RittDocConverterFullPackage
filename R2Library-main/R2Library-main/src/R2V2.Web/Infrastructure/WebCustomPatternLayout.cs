#region

using System;
using System.IO;
using System.Text;
using log4net.Util;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Infrastructure.HttpModules;

#endregion

namespace R2V2.Web.Infrastructure
{
    public class WebCustomPatternLayout : R2V2CustomPatternLayout
    {
        public WebCustomPatternLayout()
        {
            AddConverter(new ConverterInfo
                { Name = "r2SessionIdRequestId", Type = typeof(SessionIdRequestIdPatternConverter) });
            AddConverter(new ConverterInfo { Name = "r2SessionId", Type = typeof(SessionIdPatternConverter) });
            AddConverter(new ConverterInfo { Name = "r2RequestId", Type = typeof(RequestIdPatternConverter) });
            AddConverter(new ConverterInfo { Name = "r2InstitutionId", Type = typeof(InstitutionIdPatternConverter) });
            AddConverter(new ConverterInfo { Name = "r2UserId", Type = typeof(UserIdPatternConverter) });
            AddConverter(new ConverterInfo { Name = "r2EmailExtra", Type = typeof(EmailExtraPatternConverter) });
        }
    }

    // Change 'protected override' to 'public override' for all Convert methods overriding PatternConverter.Convert

    public class SessionIdRequestIdPatternConverter : PatternConverter
    {
        public override void Convert(TextWriter writer, object state)
        {
            try
            {
                var requestLoggerData = RequestLoggerModule.GetRequestLoggerData();
                var ids = requestLoggerData != null
                    ? $"{requestLoggerData.AspSessionId}/{requestLoggerData.RequestId}"
                    : "n/a";
                writer.Write(ids);
            }
            catch (Exception ex)
            {
                writer.Write($"ex: {ex.Message}");
            }
        }
    }

    public class SessionIdPatternConverter : PatternConverter
    {
        public override void Convert(TextWriter writer, object state)
        {
            try
            {
                var requestLoggerData = RequestLoggerModule.GetRequestLoggerData();
                var id = requestLoggerData != null ? $"{requestLoggerData.AspSessionId}" : "";
                writer.Write(id);
            }
            catch (Exception ex)
            {
                writer.Write($"ex: {ex.Message}");
            }
        }
    }

    public class RequestIdPatternConverter : PatternConverter
    {
        public override void Convert(TextWriter writer, object state)
        {
            try
            {
                var requestLoggerData = RequestLoggerModule.GetRequestLoggerData();
                var id = requestLoggerData != null ? $"{requestLoggerData.RequestId}" : "";
                writer.Write(id);
            }
            catch (Exception ex)
            {
                writer.Write($"ex: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// </summary>
    public class InstitutionIdPatternConverter : PatternConverter
    {
        public override void Convert(TextWriter writer, object state)
        {
            try
            {
                var requestLoggerData = RequestLoggerModule.GetRequestLoggerData();
                var id = requestLoggerData != null ? $"{requestLoggerData.InstitutionId}" : null;
                writer.Write(id);
            }
            catch (Exception ex)
            {
                writer.Write($"ex: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// </summary>
    public class UserIdPatternConverter : PatternConverter
    {
        public override void Convert(TextWriter writer, object state)
        {
            try
            {
                var requestLoggerData = RequestLoggerModule.GetRequestLoggerData();
                var id = requestLoggerData != null ? $"{requestLoggerData.UserId}" : null;
                writer.Write(id);
            }
            catch (Exception ex)
            {
                writer.Write($"ex: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// </summary>
    public class EmailExtraPatternConverter : PatternConverter
    {
        public override void Convert(TextWriter writer, object state)
        {
            try
            {
                var requestLoggerData = RequestLoggerModule.GetRequestLoggerData();
                var extra = new StringBuilder();
                if (requestLoggerData != null)
                {
                    extra.AppendLine($"Session Id: {requestLoggerData.AspSessionId}");
                    extra.AppendLine($"Request Id: {requestLoggerData.RequestId}");
                    extra.AppendLine($"Institution Id: {requestLoggerData.InstitutionId}");
                    extra.AppendLine($"Institution: {requestLoggerData.InstitutionName}");
                    extra.AppendLine($"User Id: {requestLoggerData.UserId}");
                    extra.AppendLine($"User: {requestLoggerData.UserName}");
                }
                else
                {
                    extra.AppendLine("RequestLoggerDate was null!");
                }

                writer.Write(extra.ToString());
            }
            catch (Exception ex)
            {
                writer.Write($"ex: {ex.Message}");
            }
        }
    }
}