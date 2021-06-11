using System;
using Horth.Service.Email.Scheduler.Jobs;
using Horth.Service.Email.Scheduler.Model;
using Horth.Service.Email.Scheduler.Quartz;
using Horth.Service.Email.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneOffice.Scheduler.Job.Shared;
using OneOffice.Server.Scheduler.Services;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace Horth.Service.Email.Scheduler
{
    public static class Startup
    {

        // This method gets called by the runtime. Use this method to add services to the container.
        public  static void ConfigureSchedulerServices(this IServiceCollection services, AppSettings appSettings)
        {
            services.AddDbContext<SchedulerServiceDbContext>(options => options.UseSqlite(appSettings.ConnectionString));
            services.AddScoped<ISchedulerResultUnitOfWork, SchedulerResultUnitOfWork>();

            // Add Quartz services
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddHostedService<QuartzHostedService>();
            services.AddSingleton<QuartzJobRunner>();

            services.AddScoped<MonitorEmailJob>();
            services.AddScoped<RetryQueue>();

            services.AddSingleton(new JobSchedule(typeof(MonitorEmailJob), "0 0/5 * * * ?"));              // Fire every 5 minutes
            services.AddSingleton(new JobSchedule(typeof(RetryQueue), "0 0/10 * * * ?")); // Fire every 10 minutes

        }

    }
}
