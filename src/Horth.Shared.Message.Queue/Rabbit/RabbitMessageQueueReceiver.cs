using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.Exceptions;
using Horth.Service.Email.Shared.MsgQueue;
using Horth.Service.Email.Shared.MsgQueue.Rabbit;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using ConnectionFactory = RabbitMQ.Client.ConnectionFactory;
using IConnection = RabbitMQ.Client.IConnection;

namespace Irc.Infrastructure.Services.Queue
{
    public class RabbitMessageQueueReceiver : IrcMessageQueueReceiver
    {
        private IConnection _service;
        private IModel _channel;
        private IAsyncSubscription _subscription;
        private IIrcMessageQueueService _messageQueueService;

        public RabbitMessageQueueReceiver(IIrcMessageQueueService messageQueueService, AppSettings appSettings)
            : base(appSettings)
        {
            Log.Logger.Debug($"Rabbit MQ Receiver Constructor");
            _messageQueueService = messageQueueService;
        }


        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Logger.Information($"Rabbit MQ Receiver Stop");
            return Task.CompletedTask;
        }

        public override void Initialize(IrcMessageQueueMessage.MsgService queueName)
        {
            try
            {
                var queue = new RabbitQueueName(queueName);
                Log.Logger.Debug($"Rabbit MQ Receiver Constructor");

                var factory = new ConnectionFactory()
                {
                    HostName = AppSettings.RabbitMqServer,
                    UserName = AppSettings.RabbitMqUserName,
                    Password = AppSettings.RabbitMqPassword
                };

                _service = factory.CreateConnection();
                _channel = _service.CreateModel();
                _channel.ExchangeDeclare(queue.WorkerExchange, "direct");
                _channel.QueueDeclare
                (
                    queue.WorkerQueue, true, false, false,
                    new Dictionary<string, object>
                    {
                        {"x-dead-letter-exchange", queue.RetryExchange},
                        {"x-dead-letter-routing-key", queue.RetryQueue}
                    }
                );
                _channel.QueueBind(queue.WorkerQueue, queue.WorkerExchange, queue.WorkerQueue, null);

                _channel.ExchangeDeclare(queue.RetryExchange, "direct");
                _channel.QueueDeclare
                (
                    queue.RetryQueue, true, false, false,null
                    //new Dictionary<string, object>
                    //{
                    //    {"x-dead-letter-exchange", queue.WorkerExchange},
                    //    {"x-dead-letter-routing-key", queue.WorkerQueue},
                    //    {"x-message-ttl", 30000},
                    //}
                );
                _channel.QueueBind(queue.RetryQueue, queue.RetryExchange, queue.RetryQueue, null);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (sender, ea) =>
                {
                    var ch = ((EventingBasicConsumer)sender)?.Model;
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var msg = JsonConvert.DeserializeObject<IrcMessageQueueMessage>(json);
                        var rc = HandleMessage(msg);
                        if(rc)
                            ch.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        else
                            ch.BasicNack(ea.DeliveryTag, false, false);
                    }
                    catch (IrcMessageQueueDeliveryException ex)
                    {
                        Log.Logger.Error(ex, "IrcMessageQueueDeliveryException message handler");
                        ch.BasicNack(ea.DeliveryTag, false, false);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "Base message handler");
                        ch.BasicNack(ea.DeliveryTag, false, false);
                    }
                };
                _channel.BasicConsume(queue.WorkerQueue, false, consumer);

                base.Initialize(queueName);
                Log.Logger.Debug($"Rabbit MQ Receiver Constructor");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Rabbit MQ Init");
            }
        }

        public override void Dispose()
        {
            if (_channel != null)
            {
                _channel.Close();
                _channel.Dispose();
            }
            _channel = null;

            if (_service != null)
            {
                _service.Close();
                _service.Dispose();
            }
            _service = null;

            if (_subscription != null)
            {
                _subscription.Unsubscribe();
                _subscription.Dispose();
            }
            _subscription = null;


            base.Dispose();
            Log.Logger.Information($"Rabbit MQ Receiver Shutdown");
        }
    }
}
