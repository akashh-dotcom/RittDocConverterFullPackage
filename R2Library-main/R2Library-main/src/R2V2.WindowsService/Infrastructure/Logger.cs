#region

using System;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.WindowsService.Infrastructure
{
    public class Logger<T>
    {
        private readonly ILog<T> _log;

        public Logger(ILog<T> log)
        {
            _log = log;
        }

        public virtual void Execute(Func<string> message)
        {
            _log.Info(message());
        }

        public virtual void Execute(Func<string> message, Exception e)
        {
            _log.Error(message(), e);
        }

        public virtual void Execute(string message)
        {
            _log.Info(message);
        }

        public virtual void Execute(string message, Exception e)
        {
            _log.Error(message, e);
        }

        public virtual void Execute(string format, Exception exception, params object[] args)
        {
            _log.ErrorFormat(format, args);
        }

        public virtual void Execute(string format, params object[] args)
        {
            _log.InfoFormat(format, args);
        }

        public virtual void Execute(IFormatProvider formatProvider, string format, params object[] args)
        {
            _log.InfoFormat(format, args);
        }

        public virtual void Execute(IFormatProvider formatProvider, string format, Exception exception,
            params object[] args)
        {
            _log.ErrorFormat(format, args);
        }
    } // Define minimal interfaces for compatibility

    public interface ILogger<T>
    {
        void Execute(string message);
    }

    public interface IFatalLogger<T> : ILogger<T>
    {
    }

    public interface IWarningLogger<T> : ILogger<T>
    {
    }

    public interface IInformationLogger<T> : ILogger<T>
    {
    }

    public interface IDebugLogger<T> : ILogger<T>
    {
    }

    public interface IErrorLogger<T> : ILogger<T>
    {
    }

    public interface IPerformanceLogger<T> : ILogger<T>
    {
    }

    public interface ISecurityLogger<T> : ILogger<T>
    {
    }

    public class FatalLogger<T> : Logger<T>, IFatalLogger<T>
    {
        public FatalLogger(ILog<T> log) : base(log)
        {
        }
    }

    public class WarningLogger<T> : Logger<T>, IWarningLogger<T>
    {
        public WarningLogger(ILog<T> log) : base(log)
        {
        }
    }

    public class InformationLogger<T> : Logger<T>, IInformationLogger<T>
    {
        public InformationLogger(ILog<T> log) : base(log)
        {
        }
    }

    public class DebugLogger<T> : Logger<T>, IDebugLogger<T>
    {
        public DebugLogger(ILog<T> log) : base(log)
        {
        }
    }

    public class ErrorLogger<T> : Logger<T>, IErrorLogger<T>
    {
        public ErrorLogger(ILog<T> log) : base(log)
        {
        }
    }

    public class PerformanceLogger<T> : Logger<T>, IPerformanceLogger<T>
    {
        public PerformanceLogger(ILog<T> log) : base(log)
        {
        }
    }

    public class SecurityLogger<T> : Logger<T>, ISecurityLogger<T>
    {
        public SecurityLogger(ILog<T> log) : base(log)
        {
        }
    }
}