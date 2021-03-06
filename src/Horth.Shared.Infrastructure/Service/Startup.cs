using System.IO;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.Service;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Horth.Service.Email.Shared
{
    public static class Startup
    {

        // This method gets called by the runtime. Use this method to add services to the container.
        public  static void ConfigureSharedServices(this IServiceCollection services, AppSettings appSettings)
        {
            Log.Logger.Debug("Configured Shared Services");
            services.AddSingleton<IServiceRegistry, ServiceRegistry>();

            if (!Directory.Exists(appSettings.DbDirectory))
            {
                Log.Logger.Warning($"New Installation? Database directory not found, creating it now");
                Directory.CreateDirectory(appSettings.DbDirectory);
            }

            //services.AddSingleton<IIrcMessageQueueService, IrcMessageQueueService>();

        }

    }
}
