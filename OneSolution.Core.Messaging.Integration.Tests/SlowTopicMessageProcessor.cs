using System;
using System.Threading;
using System.Threading.Tasks;

using OneSolution.ServiceBus.MessageTypes;


namespace OneSolution.Core.Messaging.Integration.Tests
{

    public class SlowTopicMessageProcessor : IMessageProcessor<ReleaseNotifierMessage>
    {
        private readonly SlowTopicIntegrationTests tests;
        private readonly int delay = 12;
        public SlowTopicMessageProcessor(SlowTopicIntegrationTests tests)
        {
            this.tests = tests;
        }

        public async Task ProcessMessagesAsync(ReleaseNotifierMessage payload, string lockToken, CancellationToken token)
        {
            await Task.Delay(delay * 1000);
            //tests.MessageReceived(payload);
            Console.WriteLine("Topic message received");
            //Console.WriteLine(payload.Id);
        }

    }
}
