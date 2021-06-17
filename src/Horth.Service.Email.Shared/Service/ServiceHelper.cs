using Horth.Service.Email.Shared.MsgQueue;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horth.Service.Email.Shared.Service
{
    public static class ServiceHelper
    {

        public static IServiceCollection AddEmailService(this IServiceCollection services)
        {
            services.AddSingleton<IIrcMessageQueueService, RabbitMessageQueueService>();
            services.AddSingleton<IEmailService, EmailService>();

            return services;
        }
    }
}
