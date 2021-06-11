using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.Service;
using Irc.Infrastructure.Services.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NATS.Client;
using Serilog;

namespace Horth.Service.Email.Shared.MsgQueue
{
    public abstract class MessageQueueReceiver : IrcService, IHostedService
    {
        private IConnection _service;
        private IAsyncSubscription _subscription;

        protected MessageQueueReceiver(IServiceScopeFactory serviceScopeFactory, IIrcMessageQueueService messageQueueService, AppSettings appSettings)
            : base(appSettings)
        {
            Log.Logger.Debug("MQ Receiver Constructor");
            MessageQueueService = messageQueueService;
            ServiceScopeFactory = serviceScopeFactory;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            //Do the migration asynchronously
            Log.Logger.Information("MQ Receiver Start");
            InitAsync();
            Subscribe();
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Logger.Information("MQ Receiver Stop");
            return Task.CompletedTask;
        }
        public void InitAsync()
        {
            try
            {
                Log.Logger.Debug("MQ Receiver Constructor");
                var cf = new ConnectionFactory();

                _service = cf.CreateConnection(AppSettings.NatsServerUrl);
                Log.Logger.Debug("MQ Receiver Constructor");
            }
            catch (System.Exception ex)
            {
                Log.Logger.Error(ex, "Init");
            }
        }
        public bool Subscribe()
        {
            var ret = false;
            try
            {
                Log.Logger.Debug("MQ Receiver Subscribe");
                _subscription = _service.SubscribeAsync(ServiceName.ToString());
                _subscription.MessageHandler += BaseHandleMessage;
                _subscription.Start();
                Log.Logger.Information("MQ Receiver Subscribed");
                ret = true;
            }
            catch (System.Exception ex)
            {
                Log.Logger.Error(ex, "Init");
            }
            return ret;
        }
        public override void Dispose()
        {
            if (_subscription != null)
            {
                _subscription.Unsubscribe();
                _subscription.Dispose();
            }
            _subscription = null;

            if (_service != null)
            {
                // Draining and closing a connection
                _service.Drain();

                // Closing a connection
                _service.Close();
            }
            _service = null;
            Log.Logger.Information("MQ Receiver Shutdown");
        }


        protected IIrcMessageQueueService MessageQueueService { get; }

        protected IServiceScopeFactory ServiceScopeFactory { get; }

        protected abstract IrcMessageQueueMessage.MsgService ServiceName { get; }
        protected abstract void HandleMessage(object sender, MsgHandlerEventArgs args);

        private void BaseHandleMessage(object sender, MsgHandlerEventArgs args)
        {
            try
            {
                var msgId = Encoding.UTF8.GetString(args.Message.Data);
                if (msgId.Equals(IrcMessageQueueService.HealthCheckKey))
                {
                    //args.Message.Reply=Shared.Logic.Services.MessageQueueService.HealthCheckReponse;

                    _service.Publish(args.Message.Reply, Encoding.UTF8.GetBytes(ServiceName.ToString()));
                }
                else
                {
                    HandleMessage(sender, args);
                }
            }
            catch (System.Exception ex)
            {
                Log.Logger.Error(ex, "Base message handler");
            }

        }

    }
}
