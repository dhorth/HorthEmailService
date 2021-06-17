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
    public abstract class IrcMessageQueueReceiver : IrcService, IHostedService
    {
        private MessageQueueNotificationProviderBase<IrcMessageQueueMessage>  _eventSubscribers;

        protected IrcMessageQueueReceiver(AppSettings appSettings)
            : base(appSettings)
        {
            Log.Logger.Debug($"MQ Receiver Constructor");
        }

        public string ServiceName { get; set; }
        public Action MessageHandler { get; set; }
        public bool FailOnException { get; set; }

        public  Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Log.Logger.Debug($"MQ StartAsync");
                Log.Logger.Debug($"MQ StartAsync");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "StartAsync");
            }
            return Task.FromResult(true);
        }

        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Logger.Information($"MQ Receiver Stop");
            return Task.CompletedTask;
        }
        public virtual void Initialize(IrcMessageQueueMessage.MsgService queue)
        {
            try
            {
                Log.Logger.Debug($"MQ Receiver Constructor");
                _eventSubscribers = new MessageQueueNotificationProviderBase<IrcMessageQueueMessage>(queue.ToString());
                Log.Logger.Debug($"MQ Receiver Constructor");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Init");
            }
        }
        public void Subscribe(MessageNotificationSubscriber<IrcMessageQueueMessage> subscription)
        {
            Log.Information($"Subscribe()");
            _eventSubscribers.Subscribe(subscription);
        }
        public void UnSubscribe(MessageNotificationSubscriber<IrcMessageQueueMessage> wateringEvent)
        {
            Log.Information($"UnSubscribe()");
            _eventSubscribers.UnSubscribe(wateringEvent);
        }
        public async Task SubscribeAsync(MessageNotificationSubscriber<IrcMessageQueueMessage> subscription)
        {
            Log.Information($"SubscribeAsync()");
            await _eventSubscribers.SubscribeAsync(subscription);
        }
        public async Task UnSubscribeAsync(MessageNotificationSubscriber<IrcMessageQueueMessage> wateringEvent)
        {
            Log.Information($"UnSubscribeAsync()");
            await _eventSubscribers.UnSubscribeAsync(wateringEvent);
        }

        public override void Dispose()
        {
            base.Dispose();
            Log.Logger.Information($"MQ Receiver Shutdown");
        }

        protected void Ack()
        {

        }
        protected virtual bool HandleMessage(IrcMessageQueueMessage msg)
        {
            var rc = false;
            Log.Logger.Debug($"HandleMessage({msg.Id})");
            try
            {
                rc=_eventSubscribers.EventNotification(msg, FailOnException);
                Log.Logger.Information($"HandleMessage({msg.Id}) => {rc}");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Error HandleMessage({msg.Id})");
                if (FailOnException)
                {
                    Log.Logger.Error($"Forwarding exception for message {msg.Id}");
                    throw;
                }
            }

            return rc;
        }

    }
}
