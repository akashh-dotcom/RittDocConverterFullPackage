#region

using System;
using log4net;
using log4net.Core;

#endregion

namespace R2V2.Infrastructure.Logging
{
    public interface ILog<T> : ILog
    {
        void PageTimeOk(object message);
        void PageTimeOkFormat(string format, params object[] args);
        void PageTimeWarn(object message);
        void PageTimeWarnFormat(string format, params object[] args);
        void PageTimeAlert(object message);
        void PageTimeAlertFormat(string format, params object[] args);
        void ContentThresholdWarn(object message);
        void ContentThresholdWarnFormat(string format, params object[] args);
        void WriteLogMessage(Level level, object message);
        void InfoFormat(string v, object totalSeconds);
    }

    public class Log<T> : ILog<T>
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(T));

        public void Debug(object message)
        {
            _log.Debug(message);
        }

        public void Debug(object message, Exception exception)
        {
            _log.Debug(message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            _log.DebugFormat(format, args);
        }

        public void DebugFormat(string format, object arg0)
        {
            _log.DebugFormat(format, arg0);
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            _log.DebugFormat(format, arg0, arg1);
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            _log.DebugFormat(format, arg0, arg1, arg2);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            _log.DebugFormat(provider, format, args);
        }

        public void Error(object message)
        {
            _log.Error(message);
        }

        public void Error(object message, Exception exception)
        {
            _log.Error(message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            _log.ErrorFormat(format, args);
        }

        public void ErrorFormat(string format, object arg0)
        {
            _log.ErrorFormat(format, arg0);
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            _log.ErrorFormat(format, arg0, arg1);
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            _log.ErrorFormat(format, arg0, arg1, arg2);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            _log.ErrorFormat(provider, format, args);
        }

        public void Fatal(object message)
        {
            _log.Fatal(message);
        }

        public void Fatal(object message, Exception exception)
        {
            _log.Fatal(message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            _log.FatalFormat(format, args);
        }

        public void FatalFormat(string format, object arg0)
        {
            _log.FatalFormat(format, arg0);
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            _log.FatalFormat(format, arg0, arg1);
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            _log.FatalFormat(format, arg0, arg1, arg2);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            _log.FatalFormat(provider, format, args);
        }

        public void Info(object message)
        {
            _log.Info(message);
        }

        public void Info(object message, Exception exception)
        {
            _log.Info(message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            _log.InfoFormat(format, args);
        }

        public void InfoFormat(string format, object arg0)
        {
            _log.InfoFormat(format, arg0);
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            _log.InfoFormat(format, arg0, arg1);
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            _log.InfoFormat(format, arg0, arg1, arg2);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            _log.InfoFormat(provider, format, args);
        }

        public void Warn(object message)
        {
            _log.Warn(message);
        }

        public void Warn(object message, Exception exception)
        {
            _log.Warn(message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            _log.WarnFormat(format, args);
        }

        public void WarnFormat(string format, object arg0)
        {
            _log.WarnFormat(format, arg0);
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            _log.WarnFormat(format, arg0, arg1);
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            _log.WarnFormat(format, arg0, arg1, arg2);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            _log.WarnFormat(provider, format, args);
        }

        public bool IsDebugEnabled => _log.IsDebugEnabled;
        public bool IsInfoEnabled => _log.IsInfoEnabled;
        public bool IsWarnEnabled => _log.IsWarnEnabled;
        public bool IsErrorEnabled => _log.IsErrorEnabled;
        public bool IsFatalEnabled => _log.IsFatalEnabled;

        public ILogger Logger => _log.Logger;

        public void PageTimeOk(object message)
        {
            WriteLogMessage(Level.Info, message);
        }

        public void PageTimeOkFormat(string format, params object[] args)
        {
            PageTimeOk(string.Format(format, args));
        }

        public void PageTimeWarn(object message)
        {
            WriteLogMessage(Level.Warn, message);
        }

        public void PageTimeWarnFormat(string format, params object[] args)
        {
            PageTimeWarn(string.Format(format, args));
        }

        public void PageTimeAlert(object message)
        {
            WriteLogMessage(Level.Error, message);
        }

        public void PageTimeAlertFormat(string format, params object[] args)
        {
            PageTimeAlert(string.Format(format, args));
        }

        public void ContentThresholdWarn(object message)
        {
            WriteLogMessage(Level.Warn, message);
        }

        public void ContentThresholdWarnFormat(string format, params object[] args)
        {
            ContentThresholdWarn(string.Format(format, args));
        }

        public void WriteLogMessage(Level level, object message)
        {
            var logImpl = _log.Logger;
            logImpl.Log(typeof(Log<T>), level, message, null);
        }
    }
}