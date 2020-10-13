using System;
using Microsoft.Extensions.Logging;

namespace OneSolution.Core.Messaging.Integration.Tests
{
    public class CongifurationFixture : IDisposable
    {
        public ServiceBusSettings Settings {get; private set;}
        public IServiceBusFactory Factory { get; private set;}
        public ILoggerFactory Log { get; private set;}


        public CongifurationFixture()
        {
            Settings = ConfigBuilder.GetSettings();
            Log = new LoggerFactory();
            Factory = new ServiceBusFactory(Settings, Log);
        }

        public virtual void Dispose()
        {
        }

    }



}
