using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;

using Xunit;
using FluentAssertions;

using OneSolution.ServiceBus.MessageTypes;

namespace OneSolution.Core.Messaging.Integration.Tests
{
    public class SlowTopicIntegrationTests : IDisposable
    {
        private readonly ServiceBusSettings settings = ConfigBuilder.GetSettings();
        private readonly string topic = "SlowTopic";
        private readonly IServiceBusSender<TestMessage> sender;
        private readonly IServiceBusReceiver receiver;
        private readonly IServiceBusFactory factory;
        private readonly ILoggerFactory log = new LoggerFactory();
        public SlowTopicIntegrationTests()
        {
            factory = new ServiceBusFactory(settings, log);
            sender = factory.CreateSender<TestMessage>(topic);
            receiver = factory.CreateReceiver<ReleaseNotifierMessage>(topic, new SlowTopicMessageProcessor(this));
            receiver.Start();
        }

        private TestMessage message;
        private TestMessage receivedMsg ;
        private CancellationTokenSource tokenSource;
        
        [Fact(Skip = "Very Slow")]
        public async Task Should_send_and_slow_process_message_in_15_seconds()
        {
            message = new TestMessage { Id = Guid.NewGuid().ToString() };
            receivedMsg = null;
            //await sender.SendMessagesAsync(message);
            tokenSource = new CancellationTokenSource();
            try
            {
                await Task.Delay(15 * 100000000, tokenSource.Token);
            }
            catch (TaskCanceledException) { }
            receivedMsg.Should().NotBeNull();
            receivedMsg.Id.Should().Be(message.Id);
        }

        [Fact(Skip = "Very Slow")]
        public async Task Should_not_get_message_for_slow_processing_message_in_4_seconds()
        {
            message = new TestMessage { Id = Guid.NewGuid().ToString() };
            receivedMsg = null;
            await sender.SendMessagesAsync(message);
            tokenSource = new CancellationTokenSource();
            try
            {
                await Task.Delay(4 * 1000, tokenSource.Token);
            }
            catch (TaskCanceledException) { }
            receivedMsg.Should().BeNull();
        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public void MessageReceived(TestMessage msg)
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
            receivedMsg = msg;
            if (msg.Id == message.Id)
                tokenSource.Cancel();
        }

        public void Dispose()
        {
            receiver.StopAsync().Wait();
        }

    }



}
