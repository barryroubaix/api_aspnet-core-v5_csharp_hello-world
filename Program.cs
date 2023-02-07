using dotenv.net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DotEnv.Load();
            IConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables();

            CreateHostBuilder(args, configBuilder.Build()).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfigurationRoot config) =>
            Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseConfiguration(config);
                        webBuilder.UseStartup<Startup>();
                        webBuilder.ConfigureKestrel(serverOptions =>
                        {
                            serverOptions.AddServerHeader = false;
                        });
                        webBuilder.UseUrls($"http://+:{config.GetValue<string>("PORT", "6060")}");
                    });
    }
}
