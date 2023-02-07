using System;
using App.Authorization;
using App.Middlewares;
using App.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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
            services.AddSingleton<IAuthorizationHandler, RbacHandler>();
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
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var audience =
                          Configuration.GetValue<string>("AUTH0_AUDIENCE");

                    options.Authority =
                          $"https://{Configuration.GetValue<string>("AUTH0_DOMAIN")}/";
                    options.Audience = audience;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true
                    };
                });
            services.AddAuthorization(options =>
                {
                    options.AddPolicy("read:admin-messages", policy =>
                    {
                        policy.Requirements.Add(new RbacRequirement("read:admin-messages"));
                    });
                });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var requiredVars =
                new string[] {
                    "PORT",
                    "CLIENT_ORIGIN_URL",
                    "AUTH0_DOMAIN",
                    "AUTH0_AUDIENCE",
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
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
