using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared;
using Horth.Service.Email.Shared.Exceptions;
using Horth.Service.Email.Shared.Model;
using Horth.Service.Email.Shared.MsgQueue.Rabbit;
using Irc.Infrastructure.Services.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Serilog;

namespace Horth.Service.Email.Shared.MsgQueue
{


    public class RabbitMessageQueueService : IrcMessageQueueService, IDisposable
    {
        protected IConnection MessageService;
        public RabbitMessageQueueService(AppSettings appSettings) : base(appSettings)
        {
        }
        protected override async Task<bool> InitAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var cf = new ConnectionFactory() { HostName = AppSettings.RabbitMqServer };
                    Log.Logger.Debug($"Msg Queue Service Constructor Server at {AppSettings.RabbitMqServer}");
                    PolicyHelper.Execute(() => MessageService = cf.CreateConnection());
                });
            }
            catch (Exception ex)
            {
                throw new IrcMessageQueueException("Init Message Queue services", ex);
            }

            return MessageService != null;
        }
        public override void Dispose()
        {
            if (MessageService != null)
            {
                // Closing a connection
                MessageService.Close();
                MessageService.Dispose();
            }
            Log.Logger.Debug($"Shutdown and clean up message queue");
            MessageService = null;
            base.Dispose();
        }

        protected override async Task PublishMessage(string queueName, byte[] jsonPayload, IrcMessageQueueMessage msg)
        {
            try
            {
                var queue = new RabbitQueueName(queueName);
                Log.Logger.Debug($"Rabbit Publish Message {queue.WorkerQueue}");

                using var channel = MessageService.CreateModel();
                channel.ExchangeDeclare(queue.WorkerExchange, "direct");
                channel.QueueDeclare
                (
                    queue.WorkerQueue, true, false, false,
                    new Dictionary<string, object>
                    {
                        {"x-dead-letter-exchange", queue.RetryExchange},
                        {"x-dead-letter-routing-key", queue.RetryQueue}
                    }
                );

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: queue.WorkerExchange, queue.WorkerQueue, properties, jsonPayload);
                Log.Logger.Information($"Rabbit Publish to {queue.WorkerQueue} ");
            }
            catch (Exception ex)
            {
                throw new IrcMessageQueueException($"Rabbit Publish Message ({queueName})", ex);
            }
        }

        protected override async Task RemoveAllMessages(string queue)
        {
            Log.Logger.Debug($"RemoveAllMessages({queue})");
            uint count = 0;
            try
            {
                Log.Logger.Debug($"Rabbit Publish Message {queue}");

                using var channel = MessageService.CreateModel();
                channel.QueueDeclare(queue: queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                count = channel.QueuePurge(queue);
            }
            catch (Exception ex)
            {
                throw new IrcMessageQueueException($"Rabbit Publish Message ({queue})", ex);
            }
            Log.Logger.Debug($"RemoveAllMessages({queue}) -> {count}");
        }

    }
}
