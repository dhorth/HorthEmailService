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
    public interface IIrcMessageQueueService
    {
        int ReRequestKey { get; }

        Task Publish(IrcMessageQueueMessage msg);
        //Task Retry(IrcMessageQueueMessage msg);
        //Task Failed(IrcMessageQueueMessage msg);
        //Task Process(string serviceName);

        Task<List<IrcMessageQueueMessage>> GetFailures(IrcMessageQueueMessage.MsgService serviceName);
        //Task<IList<int>> GetList(string serviceName);
        //Task<IrcMessageQueueMessage> Get(string serviceName, int id);
        //Task Remove(string serviceName, int id);
        Task RemoveAll(IrcMessageQueueMessage.MsgService serviceName);

        //Task<IList<T>> GetAll<T>(string queue) where T : IIrcMessageQueuePayload;
        //Task<T> Get<T>(string queue, int id) where T : IIrcMessageQueuePayload;

    }

    public abstract class IrcMessageQueueService : IIrcMessageQueueService, IDisposable
    {
        public int ReRequestKey => -1;
        public static string HealthCheckKey => "HealthCheck";
        public static string HealthCheckReponse => "Healthy";
        protected bool IsInitialized;

        protected AppSettings AppSettings { get; set; }

        protected IrcMessageQueueService(AppSettings appSettings)
        {
            AppSettings = appSettings;
        }


        public virtual void Dispose()
        {
            Log.Logger.Debug($"Shutdown and clean up message queue");
        }

        public async Task Publish(IrcMessageQueueMessage msg)
        {
            try
            {
                Log.Logger.Debug($"Publish Message {msg.serviceName}");
                if (string.IsNullOrWhiteSpace(msg.Service.ToString()))
                    throw new IrcDataException("Missing service name");

                if (!IsInitialized)
                    await InitAsync(msg.Service);

                Log.Logger.Debug($"Publish message ({msg.Id})");
                var json = JsonConvert.SerializeObject(msg);
                var message = Encoding.UTF8.GetBytes(json);
                await PublishMessage(message, msg);

                Log.Logger.Information($"Publish Service: {msg.Service} Id: {msg.Id}");
            }
            catch (Exception ex)
            {
                throw new IrcMessageQueueException($"Publish Message ({msg.Id})", ex);
            }
        }


        //public async Task Retry(IrcMessageQueueMessage msg)
        //{
        //    try
        //    {
        //        Log.Logger.Debug($"Retry Message {msg.serviceName} {msg.Id}");
        //        if (string.IsNullOrWhiteSpace(msg.Service.ToString()))
        //            throw new IrcDataException("Missing service name");

        //        if (!IsInitialized)
        //            await Initialize();

        //        Log.Logger.Information($"Retry {msg.Service} Id: {msg.Id}  Count: {msg.RetryCount}");
        //        await Publish(msg.serviceName, message);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new IrcMessageQueueException($"Retry Message ({msg.Id})", ex);
        //    }
        //}

        //public async Task Failed(IrcMessageQueueMessage msg)
        //{
        //    try
        //    {
        //        Log.Logger.Debug($"Failed Message {msg.serviceName} {msg.Id}");

        //        if (!IsInitialized)
        //            await Initialize();

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new IrcMessageQueueException($"Failed Message ({msg.Id})", ex);
        //    }
        //}

        //public async Task Process(string serviceName)
        //{
        //    Log.Logger.Debug($"Process message queue {serviceName}");

        //    if (!IsInitialized)
        //        await Initialize();

        //    //await PublishMessage(queue, Encoding.UTF8.GetBytes(ReRequestKey.ToString()));
        //    Log.Logger.Information($"Process {serviceName}");
        //}


        //public async Task<IList<IrcMessageQueueMessage>> GetAll(string serviceName)
        //{
        //    Log.Logger.Debug($"GetAll({serviceName})");
        //    var ret = await GetAllMessages(serviceName);
        //    Log.Logger.Information($"GetAll({serviceName}) => {ret.Count}");
        //    return ret;
        //}

        //public async Task<IList<int>> GetList(string serviceName)
        //{
        //    Log.Logger.Debug($"GetList({serviceName})");
        //    var ret = await GetMessageList(serviceName);
        //    Log.Logger.Information($"GetList({serviceName}) => {ret.Count}");
        //    return ret;
        //}

        //public async Task<IrcMessageQueueMessage> Get(string serviceName, int id)
        //{
        //    Log.Logger.Debug($"Get({serviceName}, {id})");
        //    var ret = await GetMessage(serviceName, id);
        //    Log.Logger.Information($"Get({serviceName}, {id}) -> {ret.Id}");
        //    return ret;
        //}

        //public async Task Remove(string serviceName, int id)
        //{
        //    Log.Logger.Debug($"Remove({serviceName}, {id})");
        //    await RemoveMessage(serviceName, id);
        //    Log.Logger.Information($"Remove({serviceName}, {id}) -> TRUE");
        //}

        public async Task RemoveAll(IrcMessageQueueMessage.MsgService serviceName)
        {
            Log.Logger.Debug($"RemoveAll({serviceName})");
            if (!IsInitialized)
                await InitAsync(serviceName);
            await RemoveAllMessages();
            Log.Logger.Debug($"RemoveAll({serviceName}) -> TRUE");
        }

        public async Task<List<IrcMessageQueueMessage>> GetFailures(IrcMessageQueueMessage.MsgService serviceName)
        {
            Log.Logger.Debug($"GetFailures({serviceName})");
            if (!IsInitialized)
                await InitAsync(serviceName);

            var ret=await GetDeadLetterQueue();
            Log.Logger.Debug($"GetFailures({serviceName}) -> {ret.Count}");
            return ret;
        }

        protected abstract Task<bool> InitAsync(IrcMessageQueueMessage.MsgService queue);
        protected abstract Task PublishMessage(byte[] payload, IrcMessageQueueMessage msg);
        //protected abstract Task<IList<IrcMessageQueueMessage>> GetAllMessages(string queue);
        //protected abstract Task<IList<int>> GetMessageList(string serviceName);
        //protected abstract Task<IrcMessageQueueMessage> GetMessage(string serviceName, int id);
        //protected abstract Task RemoveMessage(string serviceName, int id);
        protected abstract Task RemoveAllMessages();

        public abstract Task<List<IrcMessageQueueMessage>> GetDeadLetterQueue();
    }
}