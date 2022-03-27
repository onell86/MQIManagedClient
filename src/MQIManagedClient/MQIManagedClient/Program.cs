using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MQIManagedClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            
            var config = new MQConfig
            {
                CertStore = "*USER",
                ChiperSpec = "TLS_RSA_WITH_AES_256_GCM_SHA384",
                CertLabel = "ibmwebspheremqnazar",
                HostIp = "ibmwebsphere",
                Port = 1414,
                Channel = "DEV.APP.SVRCONN",
                QueueManagerName = "QM1",
                QueueName = "DEV.QUEUE.1"
            };
            using var cancellationTokenSource = new CancellationTokenSource();
            var client = new Client();
            var task = Task.Run(() => client.Listen(config, cancellationTokenSource.Token));
            Console.ReadLine();
            cancellationTokenSource.Cancel();
            Log.Logger.Information("Shutting down the app");
            await task;
        }
    }
}
