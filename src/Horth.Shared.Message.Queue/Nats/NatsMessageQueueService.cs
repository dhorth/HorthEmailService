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
using Irc.Infrastructure.Services.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client;
using Newtonsoft.Json;
using Serilog;
using IConnection = NATS.Client.IConnection;

namespace Horth.Service.Email.Shared.MsgQueue
{


    public class NatsMessageQueueService : IrcMessageQueueService, IDisposable
    {
        protected IConnection MessageService;
        private readonly IServiceScopeFactory _scopeFactory;
        public NatsMessageQueueService(AppSettings appSettings, IServiceScopeFactory scopeFactory):base(appSettings)
        {
            _scopeFactory = scopeFactory;
        }
        protected override async Task<bool> InitAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    ConnectionFactory cf = new ConnectionFactory();
                    Log.Logger.Debug($"Msg Queue Service Constructor Server at {AppSettings.NatsServerUrl}");
                    PolicyHelper.Execute(() => MessageService = cf.CreateConnection(AppSettings.NatsServerUrl));
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
                // Draining and closing a connection
                MessageService.Drain();

                // Closing a connection
                MessageService.Close();
            }
            Log.Logger.Debug($"Shutdown and clean up message queue");
            MessageService = null;
            base.Dispose();
        }

        protected override  async Task PublishMessage(string serviceName, byte[] payload, IrcMessageQueueMessage msg)
        {
            try
            {
                Log.Logger.Debug($"Nats Publish Message {msg.serviceName}");

                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<IMessageQueueMessageUnitOfWork>();
                    var dbMsg=await db.MessageQueueMessage.GetFirstAsync(a=> a.serviceName == msg.Service.ToString() && a.Id==msg.Id);
                    if (dbMsg == null)
                    {
                        db.MessageQueueMessage.Add(msg);
                        Log.Logger.Debug($"Add message to queue database");
                    }
                    else
                    {
                        var retryCount = msg.RetryCount + 1;
                        dbMsg.RetryCount = retryCount;
                        dbMsg.LastTry = DateTime.Now;
                        Log.Logger.Debug($"Nats retry message ({msg.Id}) Try {retryCount}");
                    }
                    db.Save();
                    Log.Logger.Debug($"Added message to queue database ({msg.Id})");
                }

                Log.Logger.Debug($"Nats Publish message ({msg.Id})");
                MessageService.Publish(msg.Service.ToString(), Encoding.UTF8.GetBytes(msg.Id.ToString()));
                Log.Logger.Information($"Nats Publish {msg.Service} Id: {msg.Id}");
            }
            catch (Exception ex)
            {
                throw new IrcMessageQueueException($"Nats Publish Message ({msg.Id})", ex);
            }
        }

        public async Task Failed(IrcMessageQueueMessage msg)
        {
            try
            {
                Log.Logger.Debug($"Nats Failed Message {msg.serviceName} {msg.Id}");

                if (MessageService == null)
                    await InitAsync();

                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<IMessageQueueMessageUnitOfWork>();
                    var dbMsg = await db.MessageQueueMessage.GetAsync(a => a.serviceName == msg.Service.ToString() && a.Id == msg.Id);
                    dbMsg.RetryCount = -1;
                    dbMsg.LastTry = DateTime.Now;
                    db.Save();
                    Log.Logger.Debug($"Nats marked message ({msg.Id}) as failed");
                }

                Log.Logger.Information($"Nats Failed {msg.serviceName} Id: {msg.Id}");
            }
            catch (Exception ex)
            {
                throw new IrcMessageQueueException($"Nats Failed Message ({msg.Id})", ex);
            }
        }

        protected  async Task<IList<IrcMessageQueueMessage>> GetAllMessages(string queue)
        {
            Log.Logger.Debug($"GetAllMessages({queue})");
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IMessageQueueMessageUnitOfWork>();
            var ret = await db.MessageQueueMessage.GetAllAsync(a => a.serviceName == queue);
            Log.Logger.Information($"GetAllMessages({queue}) => {ret.Count}");
            return ret;
        }
        protected async Task<IList<int>> GetMessageList(string serviceName)
        {
            Log.Logger.Debug($"GetMessageList({serviceName})");
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IMessageQueueMessageUnitOfWork>();
            var messages = await db.MessageQueueMessage.GetAllAsync(a => a.serviceName == serviceName);
            var ret = messages.Select(a => a.Id).ToList();
            Log.Logger.Information($"GetMessageList({serviceName}) => {ret.Count}");
            return ret;
        }
        protected async Task<IrcMessageQueueMessage> GetMessage(string serviceName, int id)
        {
            Log.Logger.Debug($"GetMessage({serviceName}, {id})");
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IMessageQueueMessageUnitOfWork>();
            var ret = await db.MessageQueueMessage.GetAsync(a => a.serviceName == serviceName && a.Id == id);
            Log.Logger.Information($"GetMessage({serviceName}, {id}) -> {ret.Id}");
            return ret;
        }

        protected async Task RemoveMessage(string serviceName, int id)
        {
            Log.Logger.Debug($"RemoveMessage({serviceName}, {id})");

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IMessageQueueMessageUnitOfWork>();
            await db.MessageQueueMessage.RemoveAsync(a => a.serviceName == serviceName && a.Id == id);
            Log.Logger.Information($"RemoveMessage({serviceName}, {id}) -> TRUE");
        }
        protected override async Task RemoveAllMessages(string queue)
        {
            Log.Logger.Debug($"RemoveAllMessages({queue})");
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IMessageQueueMessageUnitOfWork>();
            await db.MessageQueueMessage.RemoveRangeAsync(a => a.serviceName == queue);
            Log.Logger.Debug($"RemoveAllMessages({queue}) -> TRUE");
        }

        public async Task<T> Get<T>(string serviceName, int id) where T : IIrcMessageQueuePayload
        {
            Log.Logger.Debug($"Get({serviceName}, {id})");
            var ret = default(T);
            var msg = await GetMessage(serviceName, id);
            if (msg != null)
            {
                ret = (T)JsonConvert.DeserializeObject<T>(msg.Payload);
                ret.Id = id;

                Log.Logger.Information($"Get({serviceName}, {id}) -> {ret.Id}");
            }

            return ret;
        }
        public async Task<IList<T>> GetAll<T>(string serviceName) where T : IIrcMessageQueuePayload
        {
            Log.Logger.Debug($"GetAll({serviceName})");
            var messages = await GetAllMessages(serviceName);
            var ret = new List<T>();
            foreach (var msg in messages)
            {
                var obj = JsonConvert.DeserializeObject<T>(msg.Payload);
                obj.Id = msg.Id;
                ret.Add(obj);
            }
            Log.Logger.Information($"GetAll({serviceName}) -> {ret.Count}");
            return ret;
        }

    }
}
