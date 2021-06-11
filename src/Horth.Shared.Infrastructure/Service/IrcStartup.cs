using System;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Horth.Service.Email.Shared
{
    public abstract class IrcStartup
    {
        protected IrcStartup(IConfiguration configuration, string appName)
        {
            AppName=appName;
            Configuration = configuration;
            AppSettings = new AppSettings(configuration);
        }

        public string AppName { get;set;}
        public IConfiguration Configuration { get; }
        public AppSettings AppSettings { get; private set; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            Log.Logger.Information("Configure Irc Services");
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddSession()
            .AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = $"{AppName} Service", Version = "v1" }));

            services.ConfigureSharedServices(AppSettings);
            services.AddSingleton<AppSettings, AppSettings>();

        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/error");
                }

                app.UseSession();
                app.UseCors("CorsPolicy");
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.RoutePrefix = "swagger";
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{AppName} Service(v1)");
                });
                //app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseRouting();

                

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers(); // Map attribute-routed API controllers
                    endpoints.MapDefaultControllerRoute(); // Map conventional MVC controllers using the default route
                    endpoints.MapRazorPages();
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Configure");
            }
        }
    }
}
