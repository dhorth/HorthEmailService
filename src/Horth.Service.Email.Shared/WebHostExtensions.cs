using System;
using System.IO;
using Horth.Service.Email.Shared;
using Horth.Service.Email.Shared.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Serilog;

namespace Horth.Service.Email.Shared
{
    public static class WebHostHelper
    {
        public static IConfiguration GetConfiguration()
        {
            var env = "Production";
#if DEBUG
            env = "Development";
#endif

            if (File.Exists("appsettings.Shared.json"))
                Log.Logger.Debug("Using appsettings.shared.json");

            if (File.Exists($"appsettings.{env}.json"))
                Log.Logger.Debug($"Using appsettings.{ env}.json");

            if (File.Exists($"appsettings.Shared.{env}.json"))
                Log.Logger.Debug($"Using appsettings.Shared.{ env}.json");

            var builder = new ConfigurationBuilder()
                //.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Shared.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.Shared.{env}.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            return builder.Build();
        }
    }
    public static class WebHostExtensions
    {
        public static bool IsInKubernetes(this IWebHost webHost)
        {
            var cfg = webHost.Services.GetService<IConfiguration>();
            var orchestratorType = cfg.GetValue<string>("OrchestratorType");
            return orchestratorType?.ToUpper() == "K8S";
        }

        public static IApplicationBuilder MigrateDbContext<TContext>(this IApplicationBuilder app, Action<TContext, IServiceProvider> seeder = null) where TContext : DbContext
        {
            using var scope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
            if (scope == null)
                throw new IrcException("Unable to create service scope");

            var services = scope.ServiceProvider;
            var context = services.GetService<TContext>();

            try
            {
                Log.Logger.Debug($"Migrating database associated with context {typeof(TContext).Name}");
                InvokeSeeder(context, services, seeder);
                Log.Logger.Information($"Migrated database associated with context {typeof(TContext).Name}");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"An error occurred while migrating the database used on context {typeof(TContext).Name}");
                throw new IrcException("Exception migrating database", ex);
            }

            return app;
        }

        private static void InvokeSeeder<TContext>(TContext context, IServiceProvider services, Action<TContext, IServiceProvider> seeder = null)
            where TContext : DbContext
        {
            try
            {
                context.Database.Migrate();
                if (seeder != null)
                {
                    Log.Logger.Information($"Seeding database using action {seeder.Method}");
                    seeder(context, services);
                }
                else
                {
                    Log.Logger.Debug($"No database seed action provided");
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Seeding Database");
                throw new IrcException("Exception seeding database", ex);
            }

        }
    }

}
