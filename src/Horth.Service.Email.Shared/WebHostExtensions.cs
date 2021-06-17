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
