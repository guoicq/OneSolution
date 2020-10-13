using System.Collections.Generic;

namespace OneSolution.Core.Messaging
{
    public class ServiceBusSetting
    {
        public string Namespace { get; set; }
        public string AccessKey { get; set; }
        public string TopicPath { get; set; }
        public string QueueName { get; set; }
        public int LockDurationSeconds { get; set; }
        public int MaxAutoRenewSeconds { get; set; }
        
        public int MessageTTLSeconds { get; set; }

        public string ConnectionString => $"Endpoint=sb://{Namespace}.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey={AccessKey}";
        public bool IsTopic => !string.IsNullOrEmpty(TopicPath);
    }
    public class ServiceBusSettings: Dictionary<string, ServiceBusSetting>
    {
    }
}
