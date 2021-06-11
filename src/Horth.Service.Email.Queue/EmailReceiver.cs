using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Horth.Service.Email.Model;
using Horth.Service.Email.Shared;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.Exceptions;
using Horth.Service.Email.Shared.MsgQueue;
using Irc.Infrastructure.Services.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MimeKit;
using NATS.Client;
using Newtonsoft.Json;
using Polly;
using Serilog;

namespace Horth.Service.Email.Service
{
    public interface IEmailReceiverService : IDisposable
    {
        Task<bool> InitAsync();
        Task<bool> SubscribeAsync();
    }


    public class EmailReceiver : MessageNotificationSubscriber<IrcMessageQueueMessage>, IHostedService
    {
        private readonly ISendMessageService _sender;
        IrcMessageQueueReceiver _messageQueueReceiver;
        public EmailReceiver(IrcMessageQueueReceiver messageQueueReceiver, ISendMessageService sender) : base(IrcMessageQueueMessage.MsgService.Email.ToString())
        {
            _sender = sender;
            _messageQueueReceiver = messageQueueReceiver;
            _messageQueueReceiver.FailOnException = true;
            Log.Logger.Information("Email Receiver Constructor");
            messageQueueReceiver.ServiceName = IrcMessageQueueMessage.MsgService.Email.ToString();
        }


        public override bool HandleMessage(IrcMessageQueueMessage msg)
        {
            Log.Logger.Information("Email Message Received");
            Log.Logger.Information("Process Email Retry Queue");
            var emailMsg = JsonConvert.DeserializeObject<OneOfficeMailMessage>(msg.Payload);
            var ret=PolicyHelper.Execute(() => SendMail(emailMsg));
            if (ret.Outcome == OutcomeType.Failure && ret.ExceptionType.HasValue)
            {
                throw new IrcEmailException($"Failed to send email", ret.FinalException);
            }
            return ret.Outcome == OutcomeType.Successful;
        }
        protected bool SendMail(List<string> to, string subject, string html, List<string> cc = null)
        {
            var ooMsg = new OneOfficeMailMessage(to, subject, html) { Cc = cc };
            var msg = GetMimeMessage(ooMsg);
            SendMail(msg);
            return true;
        }

        protected void SendMail(OneOfficeMailMessage oneOfficeMsg)
        {
            if (oneOfficeMsg != null)
            {
                var msg = GetMimeMessage(oneOfficeMsg);
                SendMail(msg);
            }
        }

        protected void SendMail(MimeMessage mimeMessage)
        {
            if (mimeMessage != null)
                _sender.SendMail(mimeMessage);
        }

        private static MimeMessage GetMimeMessage(OneOfficeMailMessage oneOfficeMsg)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress("One Office", "oneoffice@bseco.com"));
            msg.Subject = oneOfficeMsg.Subject;
            var body = new BodyBuilder { HtmlBody = oneOfficeMsg.Body, TextBody = oneOfficeMsg.Body };
            if (string.IsNullOrWhiteSpace(body.HtmlBody))
            {
                Log.Logger.Warning($"Message body is blank Subject: {oneOfficeMsg.Subject} Body:{oneOfficeMsg.Body}");
            }

            AddToAddress(oneOfficeMsg, msg);
            AddCcAddress(oneOfficeMsg, msg);
            AddAttachments(oneOfficeMsg, msg, body);

            msg.Body = body.ToMessageBody();
            return msg;
        }

        private static void AddAttachments(OneOfficeMailMessage oneOfficeMsg, MimeMessage msg, BodyBuilder body)
        {
            if (oneOfficeMsg.Attachments is { Count: > 0 })
            {
                foreach (var attach in oneOfficeMsg.Attachments)
                {
                    var attachment = attach.FileName.Trim();
                    if (string.IsNullOrEmpty(attachment))
                        continue;

                    attachment = attach.FileData.Trim();
                    if (string.IsNullOrEmpty(attachment))
                        continue;

                    try
                    {
                        Log.Logger.Information($"Attaching {attach.FileName} to email");
                        body.Attachments.Add(attach.FileName, Convert.FromBase64String(attach.FileData), ContentType.Parse(attach.MimeContent));
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, $"Attaching {attach.FileName} to email {msg.Subject}");
                    }
                }
            }
        }

        private static void AddCcAddress(OneOfficeMailMessage oneOfficeMsg, MimeMessage msg)
        {
            if (oneOfficeMsg.Cc is not { Count: > 0 })
                return;

            foreach (var ccAddress in oneOfficeMsg.Cc)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(ccAddress))
                        continue;

                    msg.Cc.Add(new MailboxAddress(ccAddress.Trim(), ccAddress.Trim()));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"CC: {ccAddress}");
                }
            }
        }

        private static void AddToAddress(OneOfficeMailMessage oneOfficeMsg, MimeMessage msg)
        {
            foreach (var toAddress in oneOfficeMsg.To)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(toAddress))
                        continue;

                    msg.To.Add(new MailboxAddress(toAddress.Trim(), toAddress.Trim()));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"TO: {toAddress}");
                }
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _messageQueueReceiver.Initialize(IrcMessageQueueMessage.MsgService.Email.ToString());
            await _messageQueueReceiver.SubscribeAsync(this);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
