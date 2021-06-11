using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.MsgQueue;
using Horth.Service.Email.Shared.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NATS.Client;
using Serilog;

namespace Irc.Infrastructure.Services.Queue
{
    public abstract class NatsMessageQueueReceiver : IrcService, IHostedService
    {
        private IConnection _service;
        private IAsyncSubscription _subscription;
        private IIrcMessageQueueService _messageQueueService;
        private IServiceScopeFactory _serviceScopeFactory;

        protected NatsMessageQueueReceiver(IServiceScopeFactory serviceScopeFactory, IIrcMessageQueueService messageQueueService, AppSettings appSettings)
            : base(appSettings)
        {
            Log.Logger.Debug($"MQ Receiver Constructor");
            _messageQueueService = messageQueueService;
            _serviceScopeFactory = serviceScopeFactory;
        }
        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            //Do the migration asynchronously
            Log.Logger.Information($"MQ Receiver Start");
            InitAsync();
            Subscribe();
            return Task.CompletedTask;
        }
        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Logger.Information($"MQ Receiver Stop");
            return Task.CompletedTask;
        }
        public virtual void InitAsync()
        {
            try
            {
                Log.Logger.Debug($"MQ Receiver Constructor");
                ConnectionFactory cf = new ConnectionFactory();
                _service = cf.CreateConnection(AppSettings.NatsServerUrl);
                Log.Logger.Debug($"MQ Receiver Constructor");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Init");
            }
        }
        public virtual bool Subscribe()
        {
            var ret = false;
            try
            {
                Log.Logger.Debug($"MQ Receiver Subscribe");
                _subscription = _service.SubscribeAsync(ServiceName.ToString());
                _subscription.MessageHandler += BaseHandleMessage;
                _subscription.Start();
                Log.Logger.Information($"MQ Receiver Subscribed");
                ret = true;
            }
            catch (Exception ex)
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
            base.Dispose();
            Log.Logger.Information($"MQ Receiver Shutdown");
        }


        protected IIrcMessageQueueService MessageQueueService => _messageQueueService;
        protected IServiceScopeFactory ServiceScopeFactory => _serviceScopeFactory;

        protected abstract string ServiceName { get; }
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
            catch(Exception ex)
            {
                Log.Logger.Error(ex,"Base message handler");
            }

        }

    }
}
