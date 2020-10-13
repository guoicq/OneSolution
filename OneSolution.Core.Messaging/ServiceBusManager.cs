using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;

namespace OneSolution.Core.Messaging
{
    public class ServiceBusManager
    {
        private readonly ILogger logger;
        private readonly ServiceBusSetting setting;
        private readonly ManagementClient client;

        public ServiceBusManager(ServiceBusSetting setting, ILoggerFactory logger)
        {
            this.logger = logger.CreateLogger<ServiceBusManager>();
            this.setting = setting;
            CheckSetting();
            client = new ManagementClient(setting.ConnectionString);

        }

        public async Task CreateIfNotExisted()
        {
            if (setting.IsTopic)
            {
                var topicPath = setting.TopicPath;
                if (!await client.TopicExistsAsync(topicPath).ConfigureAwait(false))
                {
                    logger.LogInformation("ServiceBus: creating topic '{topicPath}'", topicPath);
                    var desc = new TopicDescription(topicPath);
                    desc.AutoDeleteOnIdle = TimeSpan.FromMinutes(5);
                    if (setting.MessageTTLSeconds > 0)
                        desc.DefaultMessageTimeToLive = TimeSpan.FromSeconds(setting.MessageTTLSeconds);
                    await client.CreateTopicAsync(desc).ConfigureAwait(false);
                }
                else
                    logger.LogInformation("ServiceBus: using existing topic '{topicPath}'", topicPath);
            }
            else 
            {
                var queueName = setting.QueueName;
                if (!await client.QueueExistsAsync(queueName).ConfigureAwait(false))
                {
                    logger.LogInformation($"ServiceBus: creating queue '{queueName}'");
                    var desc = new QueueDescription(queueName);
                    if (setting.LockDurationSeconds > 0)
                        desc.LockDuration = TimeSpan.FromSeconds(setting.LockDurationSeconds);
                    if (setting.MessageTTLSeconds > 0)
                        desc.DefaultMessageTimeToLive = TimeSpan.FromSeconds(setting.MessageTTLSeconds);
                    await client.CreateQueueAsync(desc).ConfigureAwait(false);
                }
                else
                    logger.LogInformation("ServiceBus: using existing queue '{queueName}'", queueName);
            }

        }

        public async Task AddSubscribtionIfNotExisted(string subscriptionId)
        {
            var topicPath = setting.TopicPath;

            if (!await client.SubscriptionExistsAsync(topicPath, subscriptionId).ConfigureAwait(false))
            {
                try
                {
                    var desc = new SubscriptionDescription(topicPath, subscriptionId);
                    if (setting.LockDurationSeconds > 0)
                    { 
                        desc.LockDuration = TimeSpan.FromSeconds(setting.LockDurationSeconds);
                    }
                    await client.CreateSubscriptionAsync(desc).ConfigureAwait(false);
                }
                catch (MessagingEntityAlreadyExistsException) { }
                logger.LogInformation("ServiceBus: creating subscription '{subscriptionId}'", subscriptionId);
            }
            else
                logger.LogInformation("ServiceBus: subscription existed '{subscriptionId}'", subscriptionId);

        }

        public async Task DeleteSubscribtionIfExisted(string subscriptionId)
        {
            var topicPath = setting.TopicPath;

            if (await client.SubscriptionExistsAsync(topicPath, subscriptionId).ConfigureAwait(false))
            {
                try
                {
                    await client.DeleteSubscriptionAsync(topicPath, subscriptionId).ConfigureAwait(false);
                }
                catch (Exception) { }
                logger.LogInformation("ServiceBus: deleting subscription '{subscriptionId}'", subscriptionId);
            }

        }

        public async Task DeleteIfExisted()
        {
            if (setting.IsTopic)
            {

                var topicPath = setting.TopicPath;

                if (await client.TopicExistsAsync(topicPath).ConfigureAwait(false))
                {
                    try
                    {
                        await client.DeleteTopicAsync(topicPath).ConfigureAwait(false);
                    }
                    catch (Exception) { }
                    logger.LogInformation("ServiceBus: deleting topic '{topicPath}'", topicPath);
                }
            }
            else
            {
                var queueName = setting.QueueName;

                if (await client.QueueExistsAsync(queueName).ConfigureAwait(false))
                {
                    try
                    {
                        await client.DeleteQueueAsync(queueName).ConfigureAwait(false);
                    }
                    catch (Exception) { }
                    logger.LogInformation("ServiceBus: deleting queue '{queueName}'", queueName);
                }
            }

        }

        private void CheckSetting()
        {
            if (setting.LockDurationSeconds > 0)
            {
                if (setting.LockDurationSeconds < 5)
                    setting.LockDurationSeconds = 5;
                if (setting.LockDurationSeconds > 300)
                    setting.LockDurationSeconds = 300;
            }
        }
    }
}
