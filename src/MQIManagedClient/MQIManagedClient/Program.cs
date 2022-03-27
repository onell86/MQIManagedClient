using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MQIManagedClient
{
    internal class Program
    {
        static void Main(string[] args)
        {                     
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton<Client>()
                        .AddHostedService<ClientHostedService>();
                })
                .ConfigureLogging(builder =>
                {
                    builder
                        .ClearProviders()
                        .AddSerilog(new LoggerConfiguration()
                                .MinimumLevel.Debug()
                                .WriteTo.Console()
                                .CreateLogger());
                });
    }
}
