using IBM.WMQ;
using Polly;
using Serilog;
using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Threading;

namespace MQIManagedClient
{
    internal class Client
    {
        public void Listen(MQConfig config, CancellationToken ct)
        {
            var retry = Policy
                .Handle<MQException>(ex =>
                {
                    Log.Logger.Error(ex, $"MQException caught: {ex.ReasonCode} - {ex.Message}. Reconnecting...");
                    return true;
                })
                .WaitAndRetry(20, (attempt, ctx) => TimeSpan.FromSeconds(3));
            retry.Execute(() => ListenInner(config, ct));
            Log.Logger.Information("Event listening is stopped");
        }
        private void ListenInner(MQConfig config, CancellationToken ct)
        {            
            using (var queueManager = ConnectionToQueueManager(config))
            {
                using (var queue = GetQueue(queueManager, config.QueueName))
                {
                    var messageOptions = CreateMessageOptions();
                    var exited = false;
                    while (!exited && !ct.IsCancellationRequested)
                        try
                        {
                            var msg = new MQMessage
                            {
                                Format = MQC.MQFMT_STRING,
                                CharacterSet = Encoding.UTF8.CodePage
                            };
                            Log.Logger.Information("Before message get");
                            queue.Get(msg, messageOptions);
                            Log.Logger.Information("After message get");
                            var rawMessage = msg.ReadString(msg.MessageLength);
                            Log.Logger.Information(rawMessage); 
                            msg.ClearMessage();
                        }
                        catch (MQException ex)
                        {
                            if (ex.ReasonCode == MQC.MQRC_NO_MSG_AVAILABLE) continue;
                            Log.Logger.Error(ex, $"MQException caught: {ex.ReasonCode} - {ex.Message}");
                            if (ex.ReasonCode == MQC.MQRC_CONNECTION_BROKEN || ex.ReasonCode == MQC.MQRC_XWAIT_CANCELED) throw;
                            exited = true;
                        }
                }
            }
        }

        private MQQueueManager ConnectionToQueueManager(MQConfig config)
        {
            var properties = new Hashtable();
            properties.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED);
            properties.Add(MQC.CONNECT_OPTIONS_PROPERTY, MQC.MQCNO_RECONNECT);

            if (!string.IsNullOrEmpty(config.CertStore))
                properties.Add(MQC.SSL_CERT_STORE_PROPERTY, config.CertStore);

            if (!string.IsNullOrEmpty(config.ChiperSpec))
            {
                properties.Add(MQC.SSL_CIPHER_SPEC_PROPERTY, config.ChiperSpec);
                properties.Add(MQC.SSL_CIPHER_SUITE_PROPERTY, config.ChiperSpec);
            }


            properties.Add(MQC.SSL_RESET_COUNT_PROPERTY, config.SslResetCount);
            if (!string.IsNullOrEmpty(config.UserId))
                properties.Add(MQC.USER_ID_PROPERTY, config.UserId);

            if (!string.IsNullOrEmpty(config.UserPass))
                properties.Add(MQC.PASSWORD_PROPERTY, config.UserPass);

            if (!string.IsNullOrEmpty(config.UserId))
                properties.Add(MQC.USE_MQCSP_AUTHENTICATION_PROPERTY, true);

            properties.Add(MQC.HOST_NAME_PROPERTY, config.HostIp);
            properties.Add(MQC.PORT_PROPERTY, config.Port.ToString(CultureInfo.InvariantCulture));
            properties.Add(MQC.CHANNEL_PROPERTY, config.Channel);

            MQEnvironment.SSLCertRevocationCheck = config.SslRevocationCheck;
            if (!string.IsNullOrEmpty(config.CertLabel))
                MQEnvironment.CertificateLabel = config.CertLabel;

            MQEnvironment.CertificateValPolicy = config.CertValPolicy;
            Log.Logger.Information("Connectiong to queue manager...");
            var queueManager = new MQQueueManager(config.QueueManagerName, properties);
            Log.Logger.Information($"Connected to queue manager '{config.QueueManagerName}'");
            return queueManager;
        }

        private MQQueue GetQueue(MQQueueManager queueManager, string queueName)
        {
            Log.Logger.Information($"Accessing queue: '{queueName}' ...");
            var queue = queueManager.AccessQueue(queueName, MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING);
            Log.Logger.Information("Queue is accessible");
            return queue;
        }

        private MQGetMessageOptions CreateMessageOptions()
        {
            return new MQGetMessageOptions
            {
                WaitInterval = 1000, //MQC.MQWI_UNLIMITED,
                Options = MQC.MQGMO_FAIL_IF_QUIESCING | MQC.MQGMO_WAIT
            };
        }
    }
}
