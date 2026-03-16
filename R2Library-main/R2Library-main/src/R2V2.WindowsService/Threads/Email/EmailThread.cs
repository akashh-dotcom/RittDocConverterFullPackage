#region

using System;
using System.Threading;
using Autofac;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.WindowsService.Threads.Email
{
    public class EmailThread : ThreadBase, IR2V2Thread
    {
        private readonly ILog<EmailThread> _log;
        private EmailRelayService _emailRelayService;

        public EmailThread(ILog<EmailThread> log)
        {
            _log = log;
            StopProcessing = false;
            _log.Debug("EmailThread initialized");
        }

        public void Start()
        {
            _log.Info("EmailThread.OnStart() >>>");
            try
            {
                _thread = new Thread(StartProcessing) { Name = "email" };
                _log.Info("initialized _thread");
                _thread.Start();
                _log.Info("started _thread");
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            _log.Info("EmailThread.OnStart() <<<");
        }

        public void Stop()
        {
            _log.Info("EmailThread is now stopping...");
            StopProcessing = true;

            if (_emailRelayService != null)
            {
                _emailRelayService.Stop = true;
            }

            _log.Info("EmailThread STOPPED");
        }

        private void StartProcessing()
        {
            _log.Info("StartProcessQueue() >>>");
            try
            {
                _emailRelayService = Bootstrapper.Container.Resolve<EmailRelayService>();
                _log.Info("_emailRelayService initialized");
                _emailRelayService.EmailProcessor();
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                Thread.Sleep(1000);
                throw;
            }
            finally
            {
                _log.Info("StartProcessQueue() <<<");
            }
        }
    }
}