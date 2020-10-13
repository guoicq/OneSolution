using Microsoft.Extensions.Configuration;

namespace OneSolution.Core.Storage.Integration.Tests
{
    public class ConfigBuilder
    {
        static readonly IConfigurationRoot Config;
        
        static ConfigBuilder()
        {

            Config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();
        }

        public static StorageSettings GetStorageSettings()
        {
            var setting = new StorageSettings();
            Config.GetSection("Storage").Bind(setting);
            return setting;
        }

        public static BlobContainerSetting GetRadioPPMBlobContainerSetting()
        {
            var setting = new BlobContainerSetting();
            Config.GetSection("Storage:RadioPPMBlob").Bind(setting);
            return setting;
        }
    }
}
