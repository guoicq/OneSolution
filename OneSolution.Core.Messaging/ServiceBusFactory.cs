using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace OneSolution.Core.Messaging
{
    public class ServiceBusFactory : IServiceBusFactory
    {
        private static readonly Dictionary<string, object> serviceClients = new Dictionary<string, object>();

        private readonly ServiceBusSettings settings;
        private readonly ILoggerFactory logger;
        public ServiceBusFactory(ServiceBusSettings settings, ILoggerFactory logger)
        {
            this.settings = settings;
            this.logger = logger;

        }

        public IServiceBusSender<T> CreateSender<T>(string name) where T : class
        {

            if (serviceClients.TryGetValue(name, out var serviceClient))
                return serviceClient as IServiceBusSender<T>;

            lock (serviceClients)
            {
                if (serviceClients.TryGetValue(name, out serviceClient))
                    return serviceClient as IServiceBusSender<T>;

                var setting = settings[name];
                serviceClient = new ServiceBusSender<T>(setting, logger);
                serviceClients.Add(name, serviceClient);
            }

            return serviceClient as IServiceBusSender<T>;
        }

        public IServiceBusReceiver CreateReceiver<T>(string name, IMessageProcessor<T> processor) where T : class
        {
            var setting = settings[name];
            var serviceClient = new ServiceBusReceiver<T>(setting, processor, logger);

            return serviceClient;
        }
    }
}
