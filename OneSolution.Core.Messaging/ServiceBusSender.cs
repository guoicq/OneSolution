using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;

namespace OneSolution.Core.Messaging
{
    public class ServiceBusSender<T>: IServiceBusSender<T> where T: class
    {
        private readonly IMessageSender sender;
        private readonly ILogger logger;
        private readonly ServiceBusSetting setting;
        private readonly ServiceBusManager manager;

        public ServiceBusSender(ServiceBusSetting setting, ILoggerFactory logger)
        {
            this.logger = logger.CreateLogger<ServiceBusSender<T>>();
            this.setting = setting;
            manager = new ServiceBusManager(setting, logger);
            manager.CreateIfNotExisted().Wait();
            
            sender = new MessageSender(setting.ConnectionString, setting.IsTopic ? setting.TopicPath : setting.QueueName );
        }

        public async Task SendMessagesAsync(T payload)
        {
            string data = JsonSerializer.Serialize<T>(payload);
            var message = new Message(Encoding.UTF8.GetBytes(data));

            try
            { 
                await sender.SendAsync(message).ConfigureAwait(false);
            }
            catch(MessagingEntityNotFoundException)
            {
                try
                {
                    await manager.CreateIfNotExisted().ConfigureAwait(false);
                    await sender.SendAsync(message).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError($"ServiceBus: failed to create client: {setting.ConnectionString}: {ex}");
                }
            }
        }

        public async Task ScheduleMessageAsync(T payload, DateTimeOffset scheduleEnqueueTimeUtc)
        {
            string data = JsonSerializer.Serialize<T>(payload);
            var message = new Message(Encoding.UTF8.GetBytes(data));

            await sender.ScheduleMessageAsync(message, scheduleEnqueueTimeUtc).ConfigureAwait(false);
        }

        public async Task CloseAsync()
        {
            if (!sender.IsClosedOrClosing)
                await sender.CloseAsync().ConfigureAwait(false);
        }
    }
}
