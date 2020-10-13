using System;
using Microsoft.Extensions.Logging;

using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Threading;

namespace OneSolution.Core.Messaging.Integration.Tests
{
    public class TopicIntegrationTests : IDisposable
    {
        private readonly ServiceBusSettings settings = ConfigBuilder.GetSettings();
        private readonly TestMessage message = new TestMessage { Id = Guid.NewGuid().ToString() };
        private readonly string topic = "Topic";
        private readonly IServiceBusSender<TestMessage> sender;
        private readonly IServiceBusReceiver receiver;
        private readonly IServiceBusFactory factory;
        private readonly ILoggerFactory log = new LoggerFactory();
        private readonly int delaySeconds = 2;
        public TopicIntegrationTests()
        {
            factory = new ServiceBusFactory(settings, log);
            sender = factory.CreateSender<TestMessage>(topic);
            receiver = factory.CreateReceiver<TestMessage>(topic, new TopicMessageProcessor(this));
            receiver.Start();
        }

        private TestMessage receivedMsg ;
        private CancellationTokenSource tokenSource;
        
        [Fact]
        public async Task Should_send_and_receive_topic_message()
        {
            await sender.SendMessagesAsync(message);
            tokenSource = new CancellationTokenSource();
            try
            {
                await Task.Delay(delaySeconds * 1000, tokenSource.Token);
            }
            catch (TaskCanceledException) { }
            receivedMsg.Should().NotBeNull();
            receivedMsg.Id.Should().Be(message.Id);
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
            sender.CloseAsync().Wait();
            receiver.StopAsync().Wait();
        }

    }



}
