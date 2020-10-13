using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using OneSolution.Core.Utilities;

namespace OneSolution.Core.Messaging
{

    public class ServiceBusReceiver<T> : IServiceBusReceiver where T : class
    {
        private static readonly string subscriptionId = GetInstanceId();
        private const byte charAt = 64; // '@' 
        private readonly IMessageReceiver receiver;
        private readonly ServiceBusSetting setting;
        private readonly IMessageProcessor<T> processor;
        private readonly ILogger logger;
        private readonly bool isTopic;
        private readonly ServiceBusManager manager;

        public ServiceBusReceiver(ServiceBusSetting setting, IMessageProcessor<T> processor, ILoggerFactory logger)
        {
            this.logger = logger.CreateLogger<ServiceBusReceiver<T>>();
            this.processor = processor;
            this.setting = setting;

            isTopic = setting.IsTopic;
            manager = new ServiceBusManager(setting, logger);
            manager.CreateIfNotExisted().Wait();

            if (isTopic)
                manager.AddSubscribtionIfNotExisted(subscriptionId).Wait();
            
            receiver = new MessageReceiver(setting.ConnectionString, isTopic ? $"{setting.TopicPath}/subscriptions/{subscriptionId}" : setting.QueueName);
        }

        public void Start()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler) {
                MaxConcurrentCalls = 1,
                AutoComplete = false,
            };
            if (setting.MaxAutoRenewSeconds > 0)
                messageHandlerOptions.MaxAutoRenewDuration = TimeSpan.FromSeconds(setting.MaxAutoRenewSeconds);

            receiver.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        public async Task StopAsync()
        {
            if (!receiver.IsClosedOrClosing)
                await receiver.CloseAsync().ConfigureAwait(false);

            if (isTopic)
                await manager.DeleteSubscribtionIfExisted(subscriptionId).ConfigureAwait(false);
        }

        public async Task<DateTime> RenewLockAsync(string lockToken)
        {
            var time = await receiver.RenewLockAsync(lockToken).ConfigureAwait(false);
            return time;
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            T payload = null;
            if (message.Body[0] == charAt)
                payload = OneSolution.ServiceBus.XmlMessageExtention.ParseXmlMessage<T>(message);
            else
                payload = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(message.Body));
            var lockToken = message.SystemProperties.LockToken;
            await processor.ProcessMessagesAsync(payload, lockToken, token).ConfigureAwait(false);
            await receiver.CompleteAsync(lockToken).ConfigureAwait(false);
        }

        private async Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            if (exceptionReceivedEventArgs.Exception is MessagingEntityNotFoundException)
            {
                // reinitalize all   
                try
                {
                    await manager.CreateIfNotExisted().ConfigureAwait(false);
                    if (isTopic)
                        await manager.AddSubscribtionIfNotExisted(subscriptionId).ConfigureAwait(false);
                }
                catch(Exception ex)
                {
                    logger.LogError($"ServiceBus: failed to create client: {setting.ConnectionString}: {ex}");
                }
            }
            
            logger.LogError(exceptionReceivedEventArgs.Exception, "Message handler encountered an exception");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            logger.LogDebug("Endpoint: {Endpoint}, Entity Path: {EntityPath}, Executing Action: {Action}", context.Endpoint, context.EntityPath, context.Action);

        }

        private static string GetInstanceId()
        {
            var id = $"{SysUtils.MachineName.Replace("(", "").Replace(")", "")}_{Guid.NewGuid().ToString("N")}";
            id = id.Substring(0, 25) + id.Substring(id.Length - Math.Min(id.Length - 25, 25));
            return id;
        }
    }
}
