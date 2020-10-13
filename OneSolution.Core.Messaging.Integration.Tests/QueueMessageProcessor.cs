using System;
using System.Threading;
using System.Threading.Tasks;

namespace OneSolution.Core.Messaging.Integration.Tests
{

    public class QueueMessageProcessor : IMessageProcessor<TestMessage>
    {
        private readonly QueueFixture tests;
        public QueueMessageProcessor(QueueFixture tests)
        {
            this.tests = tests;
        }

        public Task ProcessMessagesAsync(TestMessage payload, string lockToken, CancellationToken token)
        {
            tests.MessageReceived(payload);
            Console.WriteLine("Queue message received");
            Console.WriteLine(payload.Id);
            return Task.CompletedTask;
        }

    }

}
