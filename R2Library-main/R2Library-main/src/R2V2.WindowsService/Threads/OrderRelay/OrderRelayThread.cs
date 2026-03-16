#region

using System;
using System.Threading;
using Autofac;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.WindowsService.Threads.OrderRelay
{
    public class OrderRelayThread : ThreadBase, IR2V2Thread
    {
        private readonly ILog<OrderRelayThread> _log;
        private OrderRelayService _orderRelayService;

        public OrderRelayThread(ILog<OrderRelayThread> log)
        {
            _log = log;
            StopProcessing = false;
            _log.Debug("OrderRelayThread initialized");
        }

        public void Start()
        {
            _log.Info("OrderRelayThread.OnStart() >>>");
            try
            {
                _thread = new Thread(StartProcessing) { Name = "order" };
                _log.Info("initialized _thread");
                _thread.Start();
                _log.Info("started _thread");
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            _log.Info("OrderRelayThread.OnStart() <<<");
        }

        public void Stop()
        {
            _log.Info("OrderRelayThread is now stopping...");
            StopProcessing = true;

            if (_orderRelayService != null)
            {
                _orderRelayService.Stop = true;
            }

            _log.Info("OrderRelayThread STOPPED");
        }

        private void StartProcessing()
        {
            _log.Info("StartProcessQueue() >>>");
            try
            {
                _orderRelayService = Bootstrapper.Container.Resolve<OrderRelayService>();
                _log.Info("_orderRelayService initialized");
                _orderRelayService.OrderProcessor();
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