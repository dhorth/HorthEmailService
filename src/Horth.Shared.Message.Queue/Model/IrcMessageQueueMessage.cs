using System;

namespace Irc.Infrastructure.Services.Queue
{
    public interface IIrcMessageQueuePayload
    {
        public string Id { get; set; }
    }
    public class IrcMessageQueueMessage
    {
        public enum MsgService
        {
            Email
        }


        public IrcMessageQueueMessage()
        {
        }
        public IrcMessageQueueMessage(MsgService service, string from, string payload)
        {
            Id = Guid.NewGuid().ToString();
            Service = service;
            Created=DateTime.Now;
            From = from;
            Payload = payload;
            RetryCount = 0;
        }
        public string Id { get; set; }
        public string serviceName { get; set; }

        public MsgService Service
        {
            get
            {
                Enum.TryParse(serviceName, out MsgService ret);
                return ret;
            }
            set => serviceName = value.ToString();
        }

        public string From { get; set; }
        public int RetryCount { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastTry { get; set; }
        public string Payload { get; set; }
    }
}
