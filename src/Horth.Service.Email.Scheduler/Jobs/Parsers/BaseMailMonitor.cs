using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Horth.Service.Email.Model;
using Horth.Service.Email.Shared.MsgQueue;
using Irc.Infrastructure.Services.Queue;
using Serilog;

namespace OneOffice.Scheduler.Job.Email.Jobs.Parsers
{
    public abstract class BaseMailMonitor
    {
        private readonly IIrcMessageQueueService _messageQueueService;

        public BaseMailMonitor(IIrcMessageQueueService messageQueueService)
        {
            _messageQueueService = messageQueueService;
        }
        public abstract string Key { get; }
        public abstract Task<int> ProcessMessage(OneOfficeMailMessage message);

        public virtual void Retry()
        {
        }

        protected static List<string> GetReceiptents(OneOfficeMailMessage message)
        {
            var ret = message.Cc.Select(cc => cc.ToLower()).ToList();
            foreach (var r in ret)
            {
                Log.Logger.Debug("Recipient: {0}", r);
            }
            return ret;
        }

        protected static string GetMessageText(OneOfficeMailMessage message)
        {
            string ret;
            string body = message.Body;
            if (string.IsNullOrWhiteSpace(body))
            {
                Log.Logger.Warning("Message Body text is missing");
                return "";
            }

            body = body.Replace(Environment.NewLine, "<br/>");
            ret = string.Format($"<html><body><b>Sender</b>: {message.From}<br/>" +
                                $"<b>Sent</b>: {message.Subject}<br/>" +
                                $"<b>Subject</b>: {message.Date.GetValueOrDefault():R}<br/>" +
                                $"<hr/>{body}<br/></body></html>");
            Log.Logger.Information("Message Text: {0}", ret);
            return ret;
        }

        protected async Task Publish(IrcMessageQueueMessage obj)
        {
            await _messageQueueService.Publish(obj);
        }
    }

    public class LogMessage
    {
        public string EID { get; set; }
        public string Sender { get; set; }
        public DateTime timestamp { get; set; }
        public string Subject { get; set; }
        public string Folder { get; set; }
        public string Status { get; set; }
    }
}