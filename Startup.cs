using System;
using App.Middlewares;
using App.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IMessageService, MessageService>();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(
                        Configuration.GetValue<string>("CLIENT_ORIGIN_URL"))
                    .WithHeaders(new string[] {
                            HeaderNames.ContentType,
                            HeaderNames.Authorization,

                            })
                    .WithMethods("GET")
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(86400));
                });
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var requiredVars =
                new string[] {
                    "PORT",
                    "CLIENT_ORIGIN_URL",
                };

            foreach (var key in requiredVars)
            {
                var value = Configuration.GetValue<string>(key);

                if (value == "" || value == null)
                {
                    throw new Exception($"Config variable missing: {key}.");
                }
            }

            app.UseRouting();
            app.UseErrorHandler();
            app.UseSecureHeaders();
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
