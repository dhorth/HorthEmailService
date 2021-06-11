using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Horth.Service.Email.Queue.Model;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.Email;
using Horth.Service.Email.Shared.MsgQueue;
using Horth.Service.Email.Shared.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Horth.Service.Email.TestClient
{
    class Program
    {
        public static readonly string AppName = "Horth.Service.Email.TestClient";
        static async Task<int> Main(string[] args)
        {
            var config = LoadConfiguration();
            Log.Logger = CreateSerilogLogger(config, AppName);
            Log.Logger.Information("Starting application");

            IServiceCollection _services = null;
            //setup our DI
            var hostBuilder = new HostBuilder();
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                _services = services;
                services.AddSingleton(config);

                services.AddSingleton<AppSettings>();
                services.AddSingleton<IServiceRegistry, ServiceRegistry>();
                //services.AddSingleton<IMessageQueueMessageUnitOfWork, MessageQueueMessageUnitOfWork>();
                services.AddSingleton<IIrcMessageQueueService, RabbitMessageQueueService>();
                services.AddSingleton<IEmailService, EmailService>();

                //services.AddDbContext<MessageQueueDbContext>(options => options.UseSqlite(AppSettings.ConnectionString));
            });
            var host = await hostBuilder.StartAsync();
            var emailService = host.Services.GetService<IEmailService>();
            var email = emailService.NewMessage();
            email.AddHeader("Email Test Client", "Email Request");

            email.AddSectionHeader($"Section Header");
            email.AddColumn("Row 1", 1);
            email.AddColumn("Row 2", 2);
            email.AddColumn("Row 3", 3);

            email.AddTable(new List<EmailColumn> {new EmailColumn("Col 1"), new EmailColumn("Col 2") , new EmailColumn("Col 3") });
            for (int i = 0; i < 10; i++)
            {
                email.AddRow(new List<object>{i,i*10,i*100} );
            }
            email.EndTable();
            email.AddSignature("Test Team");
            await email.Send("dhorth@horth.com", "", "Email Test Client");


            Log.Logger.Information("All done!  Press any key to exit");
            Console.ReadKey();

            return 0;
        }


        public static ILogger CreateSerilogLogger(IConfiguration configuration, string appName)
        {
            var logger = new LoggerConfiguration()
                .Enrich.WithProperty("ApplicationContext", appName)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .ReadFrom.Configuration(configuration);

            return logger.CreateLogger();
        }


        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            return builder.Build();
        }

    }
}
