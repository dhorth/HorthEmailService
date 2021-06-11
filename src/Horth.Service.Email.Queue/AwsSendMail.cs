using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Horth.Service.Email.Service;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.Exceptions;
using MimeKit;
using Serilog;

namespace Horth.Service.Email.Queue
{
    public class AwsSendMail : OneOfficeSendMailBase
    {
        public AwsSendMail(AppSettings appSettings) : base(appSettings)
        {
        }

        public override void SendMail(MimeMessage msg)
        {
            var credentials = new BasicAWSCredentials(AppSettings.AWSUsername, AppSettings.AWSPassword);

            // might want to provide credentials
            using var ses = new AmazonSimpleEmailServiceClient(credentials, RegionEndpoint.USEast1);
            var to = new List<string>();
            foreach (var t in msg.To)
                to.Add(t.Name);

            var rawMessage = new RawMessage();


            Task.Run(async () =>
            {
                await using (var ms = new MemoryStream())
                {
                    await msg.WriteToAsync(ms);
                    rawMessage.Data = ms;
                    var email = new SendRawEmailRequest
                    {
                        Source = AppSettings.SourceEmail,
                        Destinations = to,
                        RawMessage = rawMessage
                    };
                    var sendResult = await ses.SendRawEmailAsync(email);
                    var rc = sendResult.HttpStatusCode == HttpStatusCode.OK;
                    if (rc)
                    {
                        Log.Information($"Email '{msg.Subject}' sent Successfully");
                    }
                    else
                    {
                        Log.Error($"Email '{msg.Subject}' Failed.  Code: {sendResult.HttpStatusCode}");
                        throw new IrcEmailException(
                            $"Email '{msg.Subject}' Failed.  Code: {sendResult.HttpStatusCode}");
                    }
                }

            });
        }

    }
}
