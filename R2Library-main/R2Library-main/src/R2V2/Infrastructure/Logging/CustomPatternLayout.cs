#region

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using log4net;
using log4net.Core;
using log4net.Layout;
using log4net.Util;

#endregion

namespace R2V2.Infrastructure.Logging
{
    public class R2V2CustomPatternLayout : PatternLayout
    {
        public R2V2CustomPatternLayout()
        {
            AddConverter(new ConverterInfo { Name = "levelInt", Type = typeof(LevelIntCustomPatternConverter) });
            AddConverter(new ConverterInfo { Name = "exceptionHash", Type = typeof(ExceptionHashPatternConverter) });
        }
    }


    public class LevelIntCustomPatternConverter : PatternConverter
    {
        public override void Convert(TextWriter writer, object state)
        {
            var loggingEvent = state as LoggingEvent;

            writer.Write(loggingEvent?.Level.Value ?? 0);
        }
    }

    public class ExceptionHashPatternConverter : PatternConverter
    {
        public override void Convert(TextWriter writer, object state)
        {
            var loggingEvent = state as LoggingEvent;
            var exceptionHash = Log4NetPropertiesGenerator.GetExceptionHash(loggingEvent);
            writer.Write(exceptionHash);
        }
    }

    public static class Log4NetPropertiesGenerator
    {
        public static string GetExceptionHash(LoggingEvent loggingEvent)
        {
            string stackTrace = null;
            if (loggingEvent != null)
            {
                if (loggingEvent.Level >= Level.Warn)
                {
                    if (loggingEvent.ExceptionObject == null)
                    {
                        var controllerAndAction = LogicalThreadContext.Properties["controllerAndAction"];
                        if (controllerAndAction != null)
                        {
                            stackTrace = controllerAndAction.ToString();
                        }
                    }
                    else
                    {
                        stackTrace = loggingEvent.ExceptionObject.StackTrace;
                    }
                }

                if (!string.IsNullOrWhiteSpace(stackTrace))
                {
                    var bytes = Encoding.Unicode.GetBytes(stackTrace);

                    using (var hashAlgorithm = HashAlgorithm.Create("SHA1"))
                    {
                        if (hashAlgorithm != null)
                        {
                            var inArray = hashAlgorithm.ComputeHash(bytes);
                            return Convert.ToBase64String(inArray);
                        }
                    }
                }
            }

            return "";
        }
    }
}