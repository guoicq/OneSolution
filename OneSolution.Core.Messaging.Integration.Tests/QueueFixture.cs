using System;
using System.Threading.Tasks;
using System.Threading;

namespace OneSolution.Core.Messaging.Integration.Tests
{
    public class QueueFixture : CongifurationFixture, IMessageProcessor<TestMessage>
    {
        private readonly string queue = "Queue";

        public TestMessage Message { get; set; }
        public IServiceBusSender<TestMessage> Sender { get; private set; }
        public IServiceBusReceiver Receiver { get; private set; }
        public TestMessage ReceivedMsg { get; set; }
        public CancellationTokenSource TokenSource { get; set; }

        public QueueFixture():base()
        {
            Sender = Factory.CreateSender<TestMessage>(queue);
            Receiver = Factory.CreateReceiver<TestMessage>(queue, this);
            Receiver.Start();
        }


        public void MessageReceived(TestMessage msg)
        {
            ReceivedMsg = msg;
            if (msg.Id == Message.Id)
                TokenSource.Cancel();
        }

        public Task ProcessMessagesAsync(TestMessage payload, string lockToken, CancellationToken token)
        {
            MessageReceived(payload);
            Console.WriteLine("Queue message received");
            Console.WriteLine(payload.Id);
            return Task.CompletedTask;
        }


        public override void Dispose()
        {
            Sender.CloseAsync().Wait();
            Receiver.StopAsync().Wait();
        }

    }



}
