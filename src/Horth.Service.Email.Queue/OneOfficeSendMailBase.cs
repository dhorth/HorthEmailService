using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.MsgQueue;
using Irc.Infrastructure.Services.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using NATS.Client;
using Serilog;

namespace Horth.Service.Email.Service
{
    public interface ISendMessageService
    {
        void SendMail(MimeMessage msg);
    }
    
    
    public abstract class OneOfficeSendMailBase : ISendMessageService
    {
        protected AppSettings AppSettings;

        protected OneOfficeSendMailBase(AppSettings appSettings)
        {
            AppSettings = appSettings;
        }

        public abstract void SendMail(MimeMessage msg);
    }
}
