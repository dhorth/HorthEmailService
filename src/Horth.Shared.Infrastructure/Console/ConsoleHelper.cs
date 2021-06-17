using System;
using System.Threading.Tasks;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.Service;
using Horth.Shared.Infrastructure.Configuration;
using Horth.Shared.Infrastructure.Logger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Horth.Shared.Infrastructure.Console
{
    public class ConsoleHelper
    {
        public static IConfiguration InitializeConsole(string appName)
        {
            var config = ConfigurationHelper.GetConfiguration();
            Log.Logger = LoggingHelper.CreateSerilogLogger(config, appName);
            Log.Logger.Information($"Starting {appName} application");
            return config;

        }
        public static async Task<int> ConfigureServices<T>(IConfiguration config, Action<HostBuilderContext, IServiceCollection> configureDelegate) where T : ConsoleApplication
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton(config);

                services.AddSingleton<AppSettings>();
                services.AddSingleton<IServiceRegistry, ServiceRegistry>();

                configureDelegate(hostContext, services);

                services.AddSingleton<IConsoleApplication, T>();
            });

            var host = hostBuilder.Build();
            var app=host.Services.GetService<IConsoleApplication>();
            //host.Run();
            //var app = ActivatorUtilities.CreateInstance<IServiceRegistry>(host.Services);
            var ret=await app.Run();
            //return ret;
            return 0;
        }
    }

    public interface IConsoleApplication
    {
        Task<int> Run();
    }
    public abstract class ConsoleApplication :IConsoleApplication
    {
        protected ConsoleApplication() { }
        public abstract  Task<int> Run();
        

    }
}
