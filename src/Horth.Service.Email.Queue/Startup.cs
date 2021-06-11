using System;
using Horth.Service.Email.Service;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.MsgQueue;
using Irc.Infrastructure.Services.Queue;
using Microsoft.Extensions.DependencyInjection;

namespace Horth.Service.Email.Queue
{
    public static class Startup
    {

        // This method gets called by the runtime. Use this method to add services to the container.
        public  static void ConfigureQueueServices(this IServiceCollection services, AppSettings appSettings)
        {
            services.AddSingleton<IIrcMessageQueueService, RabbitMessageQueueService>();
            services.AddSingleton<IrcMessageQueueReceiver, RabbitMessageQueueReceiver>();

            services.AddHostedService<EmailReceiver>();
            services.AddSingleton<IPop3MailClient, Pop3MailClient>();
            switch (appSettings.EmailService)
            {
                case AppSettings.EmailServiceProvider.Aws:
                    services.AddSingleton<ISendMessageService, AwsSendMail>();
                    break;
                case AppSettings.EmailServiceProvider.Smtp:
                    services.AddSingleton<ISendMessageService, SmtpSendMail>();
                    break;

                default:
                    throw new Exception("Configuration Exception");
            }
        }

    }
}
