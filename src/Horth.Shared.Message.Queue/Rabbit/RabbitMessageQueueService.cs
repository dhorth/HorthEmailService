using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.Exceptions;
using Horth.Service.Email.Shared.MsgQueue.Rabbit;
using Irc.Infrastructure.Services.Queue;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace Horth.Service.Email.Shared.MsgQueue
{


    public class RabbitMessageQueueService : IrcMessageQueueService, IDisposable
    {
        RabbitQueueName queue;
        protected IConnection MessageService;
        public RabbitMessageQueueService(AppSettings appSettings) : base(appSettings)
        {
        }
        protected override Task<bool> InitAsync(IrcMessageQueueMessage.MsgService queueName)
        {
            try
            {
                queue = new RabbitQueueName(queueName);
                var cf = new ConnectionFactory() { HostName = AppSettings.RabbitMqServer };
                Log.Logger.Debug($"Msg Queue Service Constructor Server at {AppSettings.RabbitMqServer}");
                PolicyHelper.Execute(() => MessageService = cf.CreateConnection());
            }
            catch (Exception ex)
            {
                throw new IrcMessageQueueException("Init Message Queue services", ex);
            }

            return Task.FromResult(MessageService != null);
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

        protected override Task PublishMessage(byte[] jsonPayload, IrcMessageQueueMessage msg)
        {
            try
            {
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
                throw new IrcMessageQueueException($"Rabbit Publish Message ({queue.WorkerQueue})", ex);
            }
            return Task.FromResult(true);
        }

        protected override Task RemoveAllMessages()
        {
            Log.Logger.Debug($"RemoveAllMessages({queue.WorkerQueue})");
            var count = RemoveAllQueueMessages(queue.WorkerQueue);
            Log.Logger.Debug($"RemoveAllMessages({queue.WorkerQueue}) -> {count}");
            return Task.FromResult(true);
        }

        protected uint RemoveAllQueueMessages(string queue)
        {
            Log.Logger.Debug($"RemoveAllQueueMessages({queue})");
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
                throw new IrcMessageQueueException($"Rabbit Remove All Queue Messages ({queue})", ex);
            }
            Log.Logger.Debug($"RemoveAllQueueMessages({queue}) -> {count}");
            return count;
        }

        public override Task<List<IrcMessageQueueMessage>> GetDeadLetterQueue()
        {
            Log.Logger.Debug($"GetDeadLetterQueue({queue})");
            var ret = new List<IrcMessageQueueMessage>();
            try
            {
                Log.Logger.Debug($"Rabbit Publish Message {queue}");

                using var channel = MessageService.CreateModel();
                var queueDeclareResponse = channel.QueueDeclare(queue: queue.RetryQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new DefaultBasicConsumer(channel);
                while(true)
                {
                    try
                    {
                        var ea = channel.BasicGet(queue.RetryQueue, false);
                        if (ea == null)
                            break;
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var msg = JsonConvert.DeserializeObject<IrcMessageQueueMessage>(json);
                        ret.Add(msg);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "Base message handler");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "GetDeadLetterQueue");
            }
            Log.Logger.Debug($"GetDeadLetterQueue({queue}) -> {ret.Count}");
            return Task.FromResult(ret);
        }
    }
}
