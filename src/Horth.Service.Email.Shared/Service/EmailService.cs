using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Horth.Service.Email.Model;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.Email;
using Horth.Service.Email.Shared.MsgQueue;
using Irc.Infrastructure.Services.Queue;
using MimeKit;
using Newtonsoft.Json;
using Serilog;

namespace Horth.Service.Email.Shared.Service
{
    public interface IEmailReceiverService
    {
        Task<bool> InitAsync();
        Task<bool> SubscribeAsync();
    }

    public interface IEmailService
    {
        Task<bool> SendAsync(string to, string cc, string subject, string body, string attachment = "");
        Task<bool> SendAsync(string to, string cc, string subject, string body, string formData64, string fileName);
        Task<bool> SendAsync(List<string> to, List<string> cc, string subject, string body, string attachment = "");
        EmailMessage NewMessage();
        Task<List<OneOfficeMailMessage>> CheckMail();
        Task<List<OneOfficeMailMessage>> GetQueue();

    }

    public class EmailServiceName : ServiceName
    {
        public static EmailServiceName CheckMail = new EmailServiceName("CheckMail");
        public static EmailServiceName GetQueue = new EmailServiceName("GetQueue");

        public EmailServiceName(string name) : base(name)
        {
        }
    }


    public class EmailService : IrcService, IEmailService
    {
        private IIrcMessageQueueService _messageService;
        public EmailService(IIrcMessageQueueService messageService, AppSettings configuration, IServiceRegistry serviceRegistry) : base(configuration)
        {
            _messageService = messageService;
            _baseUrl = $"{serviceRegistry.GetTarget( RegisteredServiceName.Email)}/Email/api/v1";
            Log.Logger.Debug($"Email Service Constructor");
        }

        public EmailMessage NewMessage() => new EmailMessage(this, AppSettings);

        public async Task<bool> SendAsync(string to, string cc, string subject, string body, string formData64, string fileName)
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "Attachments");
            var path = Path.Combine(dir, fileName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllBytes(path, Convert.FromBase64String(formData64));
            Logger.Information($"Form data saved to {path}");

            var toList = StringToList(to);
            var ccList = StringToList(cc);
            return await SendAsync(toList, ccList, subject, body, path);
        }

        public async Task<bool> SendAsync(string to, string cc, string subject, string body, string attachment = "")
        {
            var toList = StringToList(to);
            var ccList = StringToList(cc);
            return await SendAsync(toList, ccList, subject, body, attachment);
        }

        public async Task<bool> SendAsync(List<string> to, List<string> cc, string subject, string body, string attachment = "")
        {
            var rc = false;

            await Task.Run(() =>
            {
                try
                {
                    if (to == null || to.Count <= 0)
                    {
                        Logger.Warning($"Message {subject} has no valid receipents");
                    }
                    else
                    {                        
                        var attachmentList = new List<string>();
                        if (!string.IsNullOrWhiteSpace(attachment))
                            attachmentList.Add(attachment);

                        var oneOfficeMailMessage = new OneOfficeMailMessage(to, subject, body);

                        foreach (var ccAddress in cc)
                        {
                            oneOfficeMailMessage.Cc.Add(ccAddress);
                        }

                        if (attachmentList != null && attachmentList.Count > 0)
                        {
                            foreach (var attach in attachmentList)
                            {
                                var a = attach.Trim();
                                if (string.IsNullOrEmpty(a))
                                    continue;

                                try
                                {
                                    var att = new OneOfficeMailAttachment
                                    {
                                        FileName = Path.GetFileName(a),
                                        FileData = Convert.ToBase64String(File.ReadAllBytes(a))
                                    };
                                    att.MimeContent = MimeTypes.GetMimeType(att.FileName);
                                    Log.Logger.Debug($"Attaching length={att.FileData.Length} to email");
                                    oneOfficeMailMessage.Attachments.Add(att);
                                }
                                catch (Exception ex)
                                {
                                    Log.Logger.Error(ex, $"Attaching {a} to email {subject}");
                                }
                            }
                        }
                        Logger.Information($"Sending message {subject} to: {to.First()}");
                        var msg = new IrcMessageQueueMessage(IrcMessageQueueMessage.MsgService.Email, "Scheduler", JsonConvert.SerializeObject(oneOfficeMailMessage));
                        _messageService.Publish(msg);
                        rc = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Send Message - {subject}");
                }
            });
            return rc;
        }

        public async Task<List<OneOfficeMailMessage>> CheckMail()
        {
            return await GetList<OneOfficeMailMessage>(EmailServiceName.CheckMail);
        }
        public async Task<List<OneOfficeMailMessage>> GetQueue()
        {
            return await GetList<OneOfficeMailMessage>(EmailServiceName.GetQueue);
        }

        private List<string> StringToList(string str)
        {
            var toList = new List<string>();
            try
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    if (str.Contains(";") || str.Contains(","))
                    {
                        if (str.Contains(";"))
                            toList.AddRange(str.Split(';').ToList());
                        if (str.Contains(","))
                            toList.AddRange(str.Split(',').ToList());
                    }
                    else
                        toList.Add(str);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"StringToList - {str}", ex);
            }
            return toList;
        }


    }
}
