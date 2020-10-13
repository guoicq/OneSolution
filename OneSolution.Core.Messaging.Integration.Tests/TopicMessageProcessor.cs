using System;
using System.Threading;
using System.Threading.Tasks;

namespace OneSolution.Core.Messaging.Integration.Tests
{

    public class TopicMessageProcessor : IMessageProcessor<TestMessage>
    {
        private readonly TopicIntegrationTests tests;

        public TopicMessageProcessor(TopicIntegrationTests tests)
        {
            this.tests = tests;
        }

        public Task ProcessMessagesAsync(TestMessage payload, string lockToken, CancellationToken token)
        {
            tests.MessageReceived(payload);
            Console.WriteLine("Topic message received");
            Console.WriteLine(payload.Id);
            return Task.CompletedTask;
        }

    }
}
