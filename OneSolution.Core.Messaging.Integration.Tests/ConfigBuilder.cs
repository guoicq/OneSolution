using Microsoft.Extensions.Configuration;

namespace OneSolution.Core.Messaging.Integration.Tests
{
    public class ConfigBuilder
    {
        private static readonly ServiceBusSettings setting = new ServiceBusSettings();
        static ConfigBuilder()
        {

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();

            config.GetSection("ServiceBus").Bind(setting);
        }

        public static ServiceBusSettings GetSettings()
        {
            return setting;
        }
    }
}
