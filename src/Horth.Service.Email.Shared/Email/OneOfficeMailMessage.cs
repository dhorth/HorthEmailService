using System;
using System.Collections.Generic;
using System.Net.Mail;
using Horth.Service.Email.Shared;
using Irc.Infrastructure.IRC.Code;
using Irc.Infrastructure.Services.Queue;
using Newtonsoft.Json;

namespace Horth.Service.Email.Model
{
    public class OneOfficeMailMessage : IIrcMessageQueuePayload
    {
        public OneOfficeMailMessage()
        {
        }
        public OneOfficeMailMessage(IrcMessageQueueMessage msg)
        {
            var obj = JsonConvert.DeserializeObject<OneOfficeMailMessage>(msg.Payload);
            CloneHelper.Clone(this,obj);
            Id = msg.Id;
        }
        public OneOfficeMailMessage(List<string> to, string subject, string body):base()
        {
            To = new List<string>(to);
            Cc = new List<string>();
            Bcc = new List<string>();
            Subject = subject;
            Body = body;
        }

        public int Id { get; set; }
        public string From { get; set; }

        //public string From { get; set; }
        public string Subject { get; set; }
        public MailPriority Priority { get; set; }
        public string Body { get; set; }
        public DateTime? Date { get;set;}
        public List<string> To { get; set; }
        public List<string> Cc { get; set; }
        public List<string> Bcc { get; set; }

        public ICollection<OneOfficeMailAttachment> Attachments { get; set; }
    }

    public class OneOfficeMailAttachment
    {
        public int Id { get; set; }

        public int MessageKey { get; set; }
        public string FileName { get;set;}
        public string FileData { get;set;}
        public string MimeContent { get;set;}

        public OneOfficeMailMessage Message { get; set; }
    }
}
