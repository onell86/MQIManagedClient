using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace MQIManagedClient
{
    internal class ClientHostedService : BackgroundService
    {
        private readonly Client _client;

        public ClientHostedService(Client client)
        {
            _client = client;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
            return Task.Run(() => _client.Listen(config, stoppingToken));
        }
    }
}
