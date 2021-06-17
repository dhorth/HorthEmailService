using System;
using Horth.Service.Email.Model;
using Horth.Service.Email.Queue;
using Horth.Service.Email.Queue.Model;
using Horth.Service.Email.Scheduler;
using Horth.Service.Email.Scheduler.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Horth.Service.Email.Shared;
using Horth.Service.Email.Shared.Model;
using Horth.Service.Email.Shared.MsgQueue;
using Irc.Infrastructure.Services.Queue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Horth.Service.Email.Shared.Service;
using Horth.Service.Email.Service;
using Horth.Service.Email.Shared.Configuration;

namespace Horth.Service.Email
{
    public class Startup : IrcStartup
    {
        public Startup(IConfiguration configuration) :
            base(configuration, Program.AppName)
        {
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.AddDbContext<EmailServiceDbContext>(options => options.UseSqlite(AppSettings.ConnectionString));
            services.AddScoped<IEmailUnitOfWork, EmailUnitOfWork>();

            services.AddSingleton<IIrcMessageQueueService, RabbitMessageQueueService>();
            services.AddSingleton<IrcMessageQueueReceiver, RabbitMessageQueueReceiver>();

            services.AddSingleton<IEmailService, EmailService>();
            services.AddHostedService<EmailReceiver>();
            services.AddSingleton<IPop3MailClient, Pop3MailClient>();
            switch (AppSettings.EmailService)
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
            services.ConfigureSchedulerServices(AppSettings);
        }

        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.MigrateDbContext<EmailServiceDbContext>();
            app.MigrateDbContext<SchedulerServiceDbContext>();
            //app.MigrateDbContext<MessageQueueDbContext>();
            base.Configure(app, env);
        }

    }
}
