using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQIManagedClient
{
    internal class MQConfig
    {
        public string CertStore { get; set; }
        public string ChiperSpec { get; set; }
        public int SslResetCount { get; set; } = 400000;
        public bool SslRevocationCheck { get; set; }
        public string CertLabel { get; set; }
        public int CertValPolicy { get; set; } = 2121;

        public string UserId { get; set; }
        public string UserPass { get; set; }

        public string HostIp { get; set; }
        public int Port { get; set; }
        public int ReconnectTimeOutInSec { get; set; } = 60;

        public string QueueManagerName { get; set; }
        public string Channel { get; set; }
        public string QueueName { get; set; }

        public int MessageBufferSize { get; set; } = 10000;
        public int MessageBufferTimeoutInSec { get; set; } = 10;
    }
}
