using System;
using System.Threading.Tasks;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.Exceptions;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Serilog;

namespace Horth.Service.Email.Service
{
    public class SmtpSendMail : OneOfficeSendMailBase
    {
        public SmtpSendMail(AppSettings appSettings) : base(appSettings)
        {
        }

        public override void SendMail(MimeMessage msg)
        {
            try
            {
                using var client = new SmtpClient();

                client.Connect(AppSettings.SmtpServer, AppSettings.SmtpPort);
                client.Authenticate(AppSettings.SmtpUsername, AppSettings.SmtpPassword);
                client.Send(msg);
                client.Disconnect(true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"SendMail({msg.Subject})");
                throw new IrcEmailException($"Error sending {msg.Subject} to {msg.To.ToString()}", ex);
            }

        }

    }
}
