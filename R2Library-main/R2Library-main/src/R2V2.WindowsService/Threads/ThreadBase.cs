#region

using System;
using System.Threading;
using Common.Logging;

#endregion

namespace R2V2.WindowsService.Threads
{
    public interface IR2V2Thread
    {
        Thread Thread { get; }
        void Start();
        void Stop();
    }

    public class ThreadBase
    {
        private static ILog _log;

        private int _pauseTime = 2500;

        protected Thread _thread;

        protected bool StopProcessing;

        public ThreadBase()
        {
            _log = LogManager.GetLogger(typeof(Program));
        }

        protected int ActiveExceptionCount { get; private set; }
        protected int TotalExceptionCount { get; private set; }

        public Thread Thread => _thread;

        protected void SleepThreadAfterException()
        {
            // this method will cause the thread to sleep for a period of time after an exception
            // giving any network related time to recover.
            // minimum sleep time, 1 minute, maximum sleep time 30 minutes.
            // the purpose of this method is to prevent a network related issue to send us hundreds of emails.
            if (StopProcessing)
            {
                _log.Debug("do not sleep, stop has been requested!");
                return;
            }

            ActiveExceptionCount++;
            TotalExceptionCount++;

            //TimeSpan pauseTimeSpan = new TimeSpan(0, (ExceptionCount > 30) ? 30 : ExceptionCount, 0);
            //_log.InfoFormat("Pausing thread after exception for {0} minutes ...", pauseTimeSpan.Minutes);
            //TimeSpan pauseTimeSpan = new TimeSpan(0, 0, (ExceptionCount > 5) ? 5 : ExceptionCount);
            //_log.InfoFormat("Pausing thread after exception for {0} seconds ...", pauseTimeSpan.Seconds);
            //Thread.Sleep(pauseTimeSpan);

            _pauseTime = _pauseTime * 2;
            var pauseTimeSpan = TimeSpan.FromMilliseconds(_pauseTime);
            _log.InfoFormat("Pausing thread after exception for {0:##}:{1:0#} ...", pauseTimeSpan.Minutes,
                pauseTimeSpan.Seconds);
            Thread.Sleep(pauseTimeSpan);
        }

        protected void SleepThreadAfterException(int sleepTimeInSeconds)
        {
            // this method will cause the thread to sleep for a period of time after an exception
            // giving any network related time to recover.
            // minimum sleep time, 1 minute, maximum sleep time 30 minutes.
            // the purpose of this method is to prevent a network related issue to send us hundreds of emails.
            if (StopProcessing)
            {
                _log.Debug("do not sleep, stop has been requested!");
                return;
            }

            ActiveExceptionCount++;
            TotalExceptionCount++;
            var pauseTimeSpan = new TimeSpan(0, 0, sleepTimeInSeconds);

            _log.InfoFormat("Pausing thread after exception for {0} minutes ...", pauseTimeSpan.Minutes);
            Thread.Sleep(pauseTimeSpan);
        }

        protected void ClearExceptionCounters()
        {
            ActiveExceptionCount = 0;
            _pauseTime = 5000;
        }
    }
}